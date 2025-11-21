using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Services;
using MiniHttpServer.Repositories;
using MiniHttpServer.DTOs;
using MiniHttpServer.Frimework.Settings;
using MyORMLibrary;
using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Core.HttpResponse;
using System.Net;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint : EndpointBase
    {
        private readonly UserRepository _userRepository;
        private readonly AuthService _authService;
        private readonly SecurityService _securityService = new SecurityService();

        public AuthEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);

            _userRepository = new UserRepository(context);
            _authService = new AuthService(_userRepository);
        }

        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            var model = new { Title = "Авторизация" };
            return new PageResult("Template/Page/login.thtml", model);
        }

        [HttpGet("json")]
        public IActionResult GetJson()
        {
            var user = new { Username = "Борис", Age = 23 };
            return new JsonResult(user);
        }

        // POST /auth/login
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Login) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return Json(new { message = "Login и Password обязательны" }, 400);
            }

            var hashed = _securityService.HashPassword(dto.Password);
            var user = _authService.Login(dto.Login, hashed);

            if (user == null)
            {
                return Json(new { message = "Неверный логин или пароль" }, 401);
            }

            var token = SessionStore.CreateSession(user.Id, user.Role);

            try
            {
                var cookie = new Cookie("token", token)
                {
                    Path = "/",
                    HttpOnly = false
                };
                Context.Response.Cookies.Add(cookie);
            }
            catch { }

            return Json(new
            {
                message = "Успешный вход",
                token = token,
                user = new { user.Id, user.Login, user.Email, user.Role }
            }, 200);
        }


        // POST /auth/register
        [HttpPost("register")]
        public IActionResult Register(UserRegistrationDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                Context.Response.StatusCode = 400;
                return Json(new { message = "Name, Email и Password обязательны" });
            }

            var newUser = new Model.User
            {
                Login = dto.Name,
                Email = dto.Email,
                PasswordHash = _securityService.HashPassword(dto.Password),
                Role = "User" // обычный юзер
            };

            if (!_authService.Register(newUser))
            {
                Context.Response.StatusCode = 400;
                return Json(new { message = "Пользователь с такой почтой уже существует" });
            }

            return Json(new { message = "Регистрация успешна" });
        }
    }
}
