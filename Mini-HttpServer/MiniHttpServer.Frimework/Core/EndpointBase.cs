using MiniHttpServer.Frimework.Core.HttpResponse;
using System;
using System.Net;

namespace MiniHttpServer.Frimework.Core
{
    public abstract class EndpointBase
    {
        protected HttpListenerContext Context { get; private set; }

        internal void SetContext(HttpListenerContext context)
        {
            Context = context;
        }

        protected string RequestBody { get; private set; } = string.Empty;

        protected IActionResult Page(string pathTemplate, object data) => new PageResult(pathTemplate, data);

        // даём возможность указывать статус
        protected IActionResult Json(object data, int statusCode = 200) => new JsonResult(data, statusCode);

        // Разбор query-параметра из URL 
        protected string GetQueryParameter(string name)
        {
            var url = Context?.Request?.Url;
            if (url == null)
                return string.Empty;

            var query = url.Query;
            if (string.IsNullOrWhiteSpace(query))
                return string.Empty;

            var raw = query.TrimStart('?');
            var parts = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2) continue;

                if (string.Equals(kv[0], name, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(kv[1]);
                }
            }

            return string.Empty;
        }

        // Получаем текущего пользователя по токену
        protected (int userId, string role)? GetCurrentUser()
        {
            // сначала пробуем query-параметр
            var token = GetQueryParameter("token");

            // если нет — ищем в cookie
            if (string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var cookies = Context?.Request?.Cookies;
                    if (cookies != null)
                    {
                        var cookie = cookies["token"] ?? cookies["session_token"];
                        if (cookie != null && !string.IsNullOrWhiteSpace(cookie.Value))
                        {
                            token = cookie.Value;
                        }
                    }
                }
                catch
                {
                    // игнорируем ошибки чтения cookies
                }
            }

            return SessionStore.GetUser(token);
        }
        internal void SetBody(string body)
        {
            RequestBody = body ?? string.Empty;
        }

    }
}
