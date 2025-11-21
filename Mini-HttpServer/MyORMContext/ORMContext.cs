using MiniHttpServer.Frimework.Core.Abstracts;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MyORMLibrary
{
    /// <summary>
    /// Основной контекст ORM для работы с PostgreSQL.
    /// Реализует CRUD-операции и простые LINQ-подобные запросы.
    /// </summary>
    public class ORMContext : IORMContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр контекста ORM с указанной строкой подключения.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных PostgreSQL.</param>
        public ORMContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        // =====================================================
        //                      CRUD
        // =====================================================

        /// <summary>
        /// Создает новую запись в таблице.
        /// </summary>
        /// <typeparam name="T">Тип модели (должен иметь свойство Id).</typeparam>
        /// <param name="entity">Экземпляр объекта для вставки.</param>
        /// <param name="tableName">Имя таблицы в базе данных.</param>
        /// <returns>Созданный объект с обновлённым Id.</returns>
        public T Create<T>(T entity, string tableName) where T : class
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var props = typeof(T).GetProperties();
            var columns = new List<string>();
            var parameters = new List<string>();

            foreach (var prop in props)
            {
                if (string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (typeof(T).Name == "Hotel" &&
                    string.Equals(prop.Name, "MealPlans", StringComparison.OrdinalIgnoreCase))
                    continue;

                columns.Add(prop.Name);
                parameters.Add($"@{prop.Name}");
            }

            string sql = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)}) RETURNING Id";

            using var cmd = new NpgsqlCommand(sql, connection);
            foreach (var prop in props)
            {
                if (string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (typeof(T).Name == "Hotel" &&
                    string.Equals(prop.Name, "MealPlans", StringComparison.OrdinalIgnoreCase))
                    continue;

                cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
            }

            var newId = cmd.ExecuteScalar();
            var idProp = typeof(T).GetProperty("Id");
            if (idProp != null && newId != null)
            {
                idProp.SetValue(entity, Convert.ToInt32(newId));
            }

            return entity;
        }

        /// <summary>
        /// Получает запись по её идентификатору.
        /// </summary>
        /// <typeparam name="T">Тип модели.</typeparam>
        /// <param name="id">Значение идентификатора.</param>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Объект или null, если запись не найдена.</returns>
        public T? ReadById<T>(int id, string tableName) where T : class, new()
        {
            string sql = $"SELECT * FROM {tableName} WHERE Id = @id";
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapReaderToObject<T>(reader);

            return null;
        }

        /// <summary>
        /// Получает все записи из таблицы.
        /// </summary>
        /// <typeparam name="T">Тип модели.</typeparam>
        /// <param name="tableName">Имя таблицы.</param>
        /// <returns>Список всех записей таблицы.</returns>
        public List<T> ReadByAll<T>(string tableName) where T : class, new()
        {
            var result = new List<T>();
            string sql = $"SELECT * FROM {tableName}";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
                result.Add(MapReaderToObject<T>(reader));

            return result;
        }

        /// <summary>
        /// Обновляет запись по идентификатору.
        /// </summary>
        /// <typeparam name="T">Тип модели.</typeparam>
        /// <param name="id">Значение идентификатора.</param>
        /// <param name="entity">Обновлённый объект.</param>
        /// <param name="tableName">Имя таблицы.</param>
        public void Update<T>(int id, T entity, string tableName) where T : class
        {
            if (string.IsNullOrWhiteSpace(tableName))
                tableName = typeof(T).Name.ToLower() + "s";

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var props = typeof(T).GetProperties()
                .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (typeof(T).Name.Equals("Hotel", StringComparison.OrdinalIgnoreCase))
            {
                props = props
                    .Where(p =>
                        (p.PropertyType.IsValueType || p.PropertyType == typeof(string)) ||
                        (p.PropertyType.IsArray && p.PropertyType.GetElementType() == typeof(string))
                    )
                    .ToList();
            }

            var setClauses = string.Join(", ", props.Select(p => $"{p.Name} = @{p.Name}"));
            var sql = $"UPDATE {tableName} SET {setClauses} WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            foreach (var prop in props)
                cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }



        /// <summary>
        /// Удаляет запись по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор записи.</param>
        /// <param name="tableName">Имя таблицы.</param>
        public void Delete(int id, string tableName)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            string sql = $"DELETE FROM {tableName} WHERE Id = @id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // =====================================================
        //                  LINQ-методы
        // =====================================================

        /// <summary>
        /// Возвращает первый объект, удовлетворяющий условию.
        /// </summary>
        public T? FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string tableName) where T : class, new()
        {
            string sql = BuildSqlQuery(predicate, tableName, true);
            return ExecuteQuerySingle<T>(sql);
        }

        /// <summary>
        /// Возвращает коллекцию объектов, удовлетворяющих условию.
        /// </summary>
        public IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, string tableName) where T : class, new()
        {
            string sql = BuildSqlQuery(predicate, tableName, false);
            return ExecuteQueryMultiple<T>(sql);
        }

        // =====================================================
        //              SQL-построитель и парсер
        // =====================================================

        /// <summary>
        /// Формирует SQL-запрос на основе выражения LINQ.
        /// </summary>
        private string BuildSqlQuery<T>(Expression<Func<T, bool>> predicate, string tableName, bool singleResult)
        {
            string where = ParseExpression(predicate.Body);
            string limit = singleResult ? "LIMIT 1" : "";
            return $"SELECT * FROM {tableName} WHERE {where} {limit}".Trim();
        }

        /// <summary>
        /// Рекурсивно парсит выражение LINQ в SQL-предикат.
        /// </summary>
        private string ParseExpression(Expression expr)
        {
            if (expr is InvocationExpression invoke && invoke.Expression is LambdaExpression lambda)
            {
                var map = new Dictionary<ParameterExpression, Expression>();

                for (int i = 0; i < lambda.Parameters.Count; i++)
                    map[lambda.Parameters[i]] = invoke.Arguments[i];

                var replacer = new ParameterReplacer(map);
                var inlinedBody = replacer.Visit(lambda.Body);

                return ParseExpression(inlinedBody);
            }

            if (expr is UnaryExpression conv &&
                (conv.NodeType == ExpressionType.Convert ||
                 conv.NodeType == ExpressionType.ConvertChecked))
            {
                return ParseExpression(conv.Operand);
            }

            return expr switch
            {
                BinaryExpression binary =>
                    $"{ParseExpression(binary.Left)} {GetSqlOperator(binary.NodeType)} {ParseExpression(binary.Right)}",

                MemberExpression member when member.Expression is ParameterExpression =>
                    member.Member.Name,

                MemberExpression member =>
                    FormatConstant(EvaluateExpression(member)),

                ConstantExpression constant =>
                    FormatConstant(constant.Value),

                UnaryExpression unary when unary.NodeType == ExpressionType.Not =>
                    $"NOT {ParseExpression(unary.Operand)}",

                MethodCallExpression call =>
                    ParseMethodCall(call),

                _ => throw new NotSupportedException($"Unsupported expression: {expr.NodeType}")
            };
        }
        /// <summary>
        /// Заменяет параметры лямбды на реальные аргументы при разворачивании Invocation.
        /// </summary>
        private sealed class ParameterReplacer : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, Expression> _map;

            public ParameterReplacer(Dictionary<ParameterExpression, Expression> map)
            {
                _map = map;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (_map.TryGetValue(node, out var replacement))
                    return replacement;

                return base.VisitParameter(node);
            }
        }

        /// <summary>
        /// Обрабатывает вызовы методов (Contains, StartsWith, EndsWith) в SQL.
        /// </summary>
        private string ParseMethodCall(MethodCallExpression method)
        {
            if (method.Method.DeclaringType == typeof(string))
            {
                string member = ParseExpression(method.Object);
                string argument = ParseExpression(method.Arguments[0]);

                return method.Method.Name switch
                {
                    nameof(string.Contains) => $"{member} ILIKE '%' || {argument} || '%'",
                    nameof(string.StartsWith) => $"{member} ILIKE {argument} || '%'",
                    nameof(string.EndsWith) => $"{member} ILIKE '%' || {argument}",
                    _ => throw new NotSupportedException(
                            $"Unsupported string method: {method.Method.Name}")
                };
            }

            if (method.Method.Name == "Contains" &&
                typeof(IEnumerable).IsAssignableFrom(method.Method.DeclaringType))
            {
                var collectionObj = EvaluateExpression(method.Object) as IEnumerable;
                if (collectionObj == null)
                {
                    throw new NotSupportedException(
                        "Contains on non-constant collection is not supported");
                }

                string columnSql = ParseExpression(method.Arguments[0]);

                var sqlValues = new List<string>();
                foreach (var item in collectionObj)
                {
                    sqlValues.Add(ParseExpression(Expression.Constant(item)));
                }

                if (sqlValues.Count == 0)
                    return "1 = 0";

                return $"{columnSql} IN ({string.Join(", ", sqlValues)})";
            }

            throw new NotSupportedException($"Unsupported method call: {method.Method.Name}");
        }

        /// <summary>
        /// Выполняет вычисление выражения и возвращает его значение.
        /// </summary>
        private object? EvaluateExpression(Expression expr)
        {
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expr, typeof(object)));
            return lambda.Compile().Invoke();
        }

        /// <summary>
        /// Форматирует значение для SQL-запроса.
        /// </summary>
        private string FormatConstant(object? value)
        {
            if (value == null) return "NULL";
            return value switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                bool b => b ? "TRUE" : "FALSE",
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                _ => value.ToString() ?? "NULL"
            };
        }

        /// <summary>
        /// Преобразует тип узла выражения в SQL-оператор.
        /// </summary>
        private string GetSqlOperator(ExpressionType nodeType) => nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.LessThan => "<",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };

        // =====================================================
        //                Исполнение SQL
        // =====================================================

        /// <summary>
        /// Выполняет SQL-запрос и возвращает один результат.
        /// </summary>
        private T? ExecuteQuerySingle<T>(string sql) where T : class, new()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapReaderToObject<T>(reader);
            return null;
        }

        /// <summary>
        /// Выполняет SQL-запрос и возвращает коллекцию результатов.
        /// </summary>
        private IEnumerable<T> ExecuteQueryMultiple<T>(string sql) where T : class, new()
        {
            var list = new List<T>();
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(MapReaderToObject<T>(reader));
            return list;
        }

        /// <summary>
        /// Отображает результат запроса на объект типа T.
        /// </summary>
        private T MapReaderToObject<T>(NpgsqlDataReader reader) where T : class, new()
        {
            T obj = new();
            foreach (var prop in typeof(T).GetProperties())
            {
                try
                {
                    int index = reader.GetOrdinal(prop.Name);
                    if (!reader.IsDBNull(index))
                    {
                        object value = reader.GetValue(index);
                        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
            return obj;
        }
    }
}
