using System;
using System.Collections.Concurrent;

namespace MiniHttpServer.Frimework.Core
{
    /// <summary>
    /// Простое in-memory хранилище сессий.
    /// token -> (userId, role)
    /// </summary>
    public static class SessionStore
    {
        private static readonly ConcurrentDictionary<string, (int userId, string role)> _sessions
            = new ConcurrentDictionary<string, (int userId, string role)>();

        public static string CreateSession(int userId, string role)
        {
            var token = Guid.NewGuid().ToString("N"); // случайный токен
            _sessions[token] = (userId, role);
            return token;
        }

        public static (int userId, string role)? GetUser(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            if (_sessions.TryGetValue(token, out var value))
                return value;

            return null;
        }
    }
}
