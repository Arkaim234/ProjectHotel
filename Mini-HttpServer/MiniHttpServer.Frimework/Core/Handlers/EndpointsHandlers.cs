using MiniHttpServer.Frimework.Core.Abstracts;
using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Frimework.Core.HttpResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MiniHttpServer.Frimework.Core.Handlers
{
    class EndpointsHandlers : Handler
    {
        // Основной метод обработки HTTP-запроса
        public override async Task HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request; // сам запрос
                var pathParts = request.Url?.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(); // разбиваем URL на части
                var endpointName = pathParts.FirstOrDefault(); // имя эндпоинта (первый сегмент пути)

                // Ищем класс эндпоинта с атрибутом [Endpoint]
                var endpoint = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                    .GetTypes()
                    .FirstOrDefault(t => t.GetCustomAttribute<EndpointAttribute>() != null &&
                                         IsCheckedEndpoint(t.Name, endpointName));

                // Если не найден — возвращаем 404
                if (endpoint == null)
                {
                    await WriteResponseAsync(context.Response, "Endpoint not found");
                    return;
                }

                // Находим подходящий метод эндпоинта по HTTP-методу и маршруту
                var (matchedMethod, matchedRouteValues) = endpoint.GetMethods()
                    .SelectMany(m => GetRouteCandidates(m, request.HttpMethod, endpointName)
                        .Select(r => new { Method = m, Route = r }))
                    .Select(x => TryMatchRoute(x.Route, pathParts, out var vals) ? (x.Method, vals) : (null, null))
                    .Where(x => x.Item1 != null)
                    .OrderByDescending(x => x.vals?.Count ?? 0)
                    .FirstOrDefault();

                // Если метод не найден — сообщаем
                if (matchedMethod == null)
                {
                    await WriteResponseAsync(context.Response, "Endpoint method not found");
                    return;
                }

                // Читаем тело запроса (если оно есть)
                string? body = request.HasEntityBody
                    ? await new StreamReader(request.InputStream, request.ContentEncoding).ReadToEndAsync()
                    : null;

                // Парсим данные запроса (query, form, json)
                var (queryParams, formParams, jsonDoc) = ParseRequestData(request, body);

                // Объединяем все параметры в один словарь (приоритет: route > query > form)
                var allParams = MergeParams(matchedRouteValues, queryParams, formParams);

                // Создаём экземпляр эндпоинта
                var instance = Activator.CreateInstance(endpoint);
                if (instance is EndpointBase baseEndpoint)
                {
                    baseEndpoint.SetContext(context);
                    baseEndpoint.SetBody(body ?? string.Empty);
                } // передаём контекст, если это наследник EndpointBase

                // Готовим аргументы для вызова метода
                var paramInfos = matchedMethod.GetParameters();
                var invokeArgs = paramInfos.Select(p => ResolveParameterValue(p, allParams, jsonDoc, body)).ToArray();

                // Вызываем метод эндпоинта
                var result = matchedMethod.Invoke(instance, invokeArgs);

                // Если метод асинхронный — ждём выполнения и извлекаем результат
                if (result is Task t)
                {
                    await t;
                    result = t.GetType().GetProperty("Result")?.GetValue(t);
                }

                // Отправляем результат клиенту
                await EndpointResponseHandler.HandleResultAsync(context, result);
            }
            catch (Exception ex)
            {
                // Логируем ошибку и возвращаем 500
                Console.WriteLine($"Handler exception: {ex}");
                if (context?.Response?.OutputStream?.CanWrite == true)
                {
                    context.Response.StatusCode = 500;
                    await WriteResponseAsync(context.Response, $"Server error: {ex.Message}");
                }
            }
        }

        // Настройки сериализации JSON (игнор регистра имён свойств + поддержка enum)
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // Проверка соответствия имени класса имени эндпоинта
        private bool IsCheckedEndpoint(string className, string endpointName) =>
             className.Equals(endpointName, StringComparison.OrdinalIgnoreCase)
            || className.Equals($"{endpointName}Endpoint", StringComparison.OrdinalIgnoreCase);

        // Генерация кандидатов маршрутов для метода (по атрибутам [HttpGet]/[HttpPost] и т.п.)
        private static IEnumerable<string> GetRouteCandidates(MethodInfo m, string httpMethod, string endpointName)
        {
            var attr = m.GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType().Name.Equals($"Http{httpMethod}", StringComparison.OrdinalIgnoreCase));
            if (attr == null) yield break;

            var routeProp = attr.GetType().GetProperty("Route");
            var attrRouteRaw = (routeProp?.GetValue(attr)?.ToString() ?? "").Trim('/');

            if (!string.IsNullOrEmpty(attrRouteRaw))
                yield return attrRouteRaw;

            if (!string.IsNullOrEmpty(endpointName))
                yield return string.IsNullOrEmpty(attrRouteRaw)
                    ? endpointName
                    : $"{endpointName}/{attrRouteRaw}";
        }

        // Парсинг query, form и json
        private static (Dictionary<string, string> query, Dictionary<string, string> form, JsonDocument json)
            ParseRequestData(HttpListenerRequest request, string body)
        {
            var query = ParseQueryString(request.Url?.Query); // параметры ?a=1&b=2
            var form = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            JsonDocument json = null;

            if (!string.IsNullOrEmpty(body))
            {
                if (request.ContentType?.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true)
                {
                    foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = pair.Split('=', 2);
                        if (kv.Length == 2)
                            form[WebUtility.UrlDecode(kv[0])] = WebUtility.UrlDecode(kv[1]);
                    }
                }
                else if (request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try { json = JsonDocument.Parse(body); } catch { }
                }
            }
            return (query, form, json);
        }

        // Объединение словарей параметров (route → query → form)
        private static Dictionary<string, string> MergeParams(params Dictionary<string, string>[] sources) =>
            sources.Where(s => s != null)
                   .SelectMany(s => s)
                   .GroupBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                   .ToDictionary(g => g.Key, g => g.First().Value, StringComparer.OrdinalIgnoreCase);

        // Разбор строки запроса (query)
        private static Dictionary<string, string> ParseQueryString(string rawQuery)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(rawQuery)) return dict;
            var q = rawQuery.TrimStart('?');
            foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                dict[WebUtility.UrlDecode(kv[0])] = kv.Length == 2 ? WebUtility.UrlDecode(kv[1]) : "";
            }
            return dict;
        }

        // Подбор значения для аргумента метода
        private static object ResolveParameterValue(ParameterInfo p, Dictionary<string, string> allParams, JsonDocument jsonDoc, string body)
        {
            // 1) route/query/form параметры
            if (allParams.TryGetValue(p.Name, out var strVal) && TryConvert(strVal, p.ParameterType, out var converted))
                return converted;

            // 2) простые поля из JSON
            if (jsonDoc != null && IsSimpleType(p.ParameterType) &&
                jsonDoc.RootElement.TryGetProperty(p.Name, out var prop) &&
                ConvertJsonElement(prop, p.ParameterType, out var val))
                return val;

            // 3) сложные объекты из JSON
            if (jsonDoc != null && !IsSimpleType(p.ParameterType))
            {
                try
                {
                    var json = jsonDoc.RootElement.TryGetProperty(p.Name, out var sub)
                        ? sub.GetRawText() : body;
                    return JsonSerializer.Deserialize(json, p.ParameterType, JsonSerializerOptions);
                }
                catch { }
            }

            // 4) если ничего не подошло — значение по умолчанию
            return GetDefault(p.ParameterType);
        }

        // Проверка соответствия маршрута шаблону
        private static bool TryMatchRoute(string template, string[] actualParts, out Dictionary<string, string> routeValues)
        {
            routeValues = new(StringComparer.OrdinalIgnoreCase);

            var tplParts = template.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // ❗ Требуем ТОЧНОЕ совпадение кол-ва сегментов
            if (tplParts.Length != actualParts.Length)
                return false;

            for (int i = 0; i < tplParts.Length; i++)
            {
                var tpl = tplParts[i];
                var actual = actualParts[i];

                // Параметр {id}
                if (tpl.StartsWith("{") && tpl.EndsWith("}"))
                {
                    var name = tpl[1..^1];
                    routeValues[name] = WebUtility.UrlDecode(actual);
                    continue;
                }

                // Если сегмент — не параметр, сравниваем строго
                if (!tpl.Equals(actual, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        // Проверка простого типа (int, string, Guid, DateTime и т.п.)
        private static bool IsSimpleType(Type t)
        {
            var u = Nullable.GetUnderlyingType(t) ?? t;
            return u.IsPrimitive || u == typeof(string) || u == typeof(decimal)
                || u == typeof(DateTime) || u == typeof(Guid) || u.IsEnum;
        }

        // Преобразование строки к нужному типу
        private static bool TryConvert(string str, Type targetType, out object result)
        {
            result = null;
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            try
            {
                if (underlying == typeof(string)) { result = str; return true; }
                if (underlying.IsEnum) { result = Enum.Parse(underlying, str, true); return true; }
                if (underlying == typeof(Guid)) { result = Guid.Parse(str); return true; }
                if (underlying == typeof(DateTime)) { result = DateTime.Parse(str); return true; }
                result = Convert.ChangeType(str, underlying);
                return true;
            }
            catch { return false; }
        }

        // Преобразование JSON-элемента в C#-тип
        private static bool ConvertJsonElement(JsonElement el, Type targetType, out object converted)
        {
            converted = null;
            if (targetType == typeof(string))
            {
                converted = el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText();
                return true;
            }
            var val = el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText();
            return TryConvert(val?.Trim('"'), targetType, out converted);
        }

        // Значение по умолчанию для типа
        private static object GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;

        // Отправка текстового ответа клиенту
        private static async Task WriteResponseAsync(HttpListenerResponse response, string content)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(content ?? "");
            response.ContentLength64 = buffer.Length;
            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);
            await output.FlushAsync();
        }
    }
}
