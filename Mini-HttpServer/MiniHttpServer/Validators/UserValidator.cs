using MiniHttpServer.DTOs;
using MiniHttpServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniHttpServer.Validators
{
    public static class UserValidator
    {
        /// <summary>
        /// Проверяет корректность email с использованием регулярного выражения
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Требование ТЗ: регулярное выражение №1
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        /// <summary>
        /// Проверяет силу пароля (минимум 6 символов)
        /// </summary>
        public static bool IsPasswordStrong(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        /// <summary>
        /// Проверяет логин (допустимы только буквы, цифры, подчёркивание, минимум 3 символа)
        /// </summary>
        public static bool IsValidLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return false;

            // Требование ТЗ: регулярное выражение №2
            return Regex.IsMatch(login, @"^[a-zA-Z0-9_]{3,20}$");
        }

        /// <summary>
        /// Полная валидация пользователя
        /// </summary>
        public static string ValidateUser(UserRegistrationDto user)
        {
            if (!IsValidLogin(user.Name))
                return "Login must be 3–20 characters, letters, digits or underscore only";

            if (!IsValidEmail(user.Email))
                return "Invalid email format";

            if (!IsPasswordStrong(user.Password))
                return "Password must be at least 6 characters";

            return string.Empty; 
        }
    }
}
