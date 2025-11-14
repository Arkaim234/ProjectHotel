using MiniHttpServer.Model;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo;

        public AuthService(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public User? Login(string login, string passwordHash)
        {
            var user = _userRepo.GetByLogin(login);
            if (user == null) return null;

            return user.PasswordHash == passwordHash ? user : null;
        }

        public bool Register(User user)
        {
            if (_userRepo.GetByEmail(user.Email) != null)
                return false;

            _userRepo.Add(user);
            return true;
        }
    }
}
