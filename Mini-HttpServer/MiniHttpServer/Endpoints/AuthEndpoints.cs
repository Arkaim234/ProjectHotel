using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Services;
using MiniHttpServer.Repositories;
using MiniHttpServer.DTOs;
using MiniHttpServer.Frimework.Settings;
using MyORMLibrary;
using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Core.HttpResponse;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint : EndpointBase
    {
        private readonly UserRepository _userRepository;
        private readonly UserService _userService;
        private readonly SecurityService _securityService = new SecurityService();

        public AuthEndpoint()
        {
            // Создаём ORM-контекст вручную (поскольку DI нет)
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);

            _userRepository = new UserRepository(context);
            _userService = new UserService(_userRepository, _securityService);
        }

        /// <summary>
        /// GET /auth/login
        /// Возвращает HTML-страницу авторизации.
        /// </summary>
        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            var model = new { Title = "Авторизация" };
            return new PageResult("Template/Page/login.thtml", model);
        }

        /// <summary>
        /// GET /auth/json
        /// Тестовый JSON-эндпоинт, не используется в основной системе.
        /// </summary>
        [HttpGet("json")]
        public IActionResult GetJson()
        {
            var user = new { Username = "Борис", Age = 23 };
            return new JsonResult(user);
        }

        /// <summary>
        /// POST /auth/login
        /// Принимает JSON с логином и паролем.
        /// Пример: { "Login": "...", "Password": "..." }
        /// Возвращает данные пользователя при успешной аутентификации,
        /// либо ошибку 401 при неверных данных.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
            {
                Context.Response.StatusCode = 400;
                return Json(new { message = "Login и Password обязательны" });
            }

            if (_userService.Authenticate(dto.Login, dto.Password, out var user))
            {
                return Json(new
                {
                    message = "Успешный вход",
                    user = new { user.Id, user.Login, user.Email, user.Role }
                });
            }

            Context.Response.StatusCode = 401;
            return Json(new { message = "Неверный логин или пароль" });
        }

        /// <summary>
        /// POST /auth/register
        /// Регистрирует нового пользователя.
        /// Принимает JSON:
        /// { "Name": "логин", "Email": "почта", "Password": "пароль" }
        /// Возвращает успех или 400 с текстом ошибки.
        /// </summary>
        [HttpPost("register")]
        public IActionResult Register(UserRegistrationDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name) ||
                string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                Context.Response.StatusCode = 400;
                return Json(new { message = "Name, Email и Password обязательны" });
            }

            var error = _userService.Register(dto);
            if (!string.IsNullOrEmpty(error))
            {
                Context.Response.StatusCode = 400;
                return Json(new { message = error });
            }

            return Json(new { message = "Регистрация успешна" });
        }
    }
}
