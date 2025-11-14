using MiniHttpServer.Model;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    /// <summary>
    /// Repository for working with User entities.
    /// Encapsulates common queries such as retrieving by login or email.
    /// </summary>
    public class UserRepository : OrmRepositories<User>
    {
        public UserRepository(IORMContext context) : base(context, "Users") { }

        public UserRepository(string connectionString) : base(connectionString, "Users") { }

        /// <summary>
        /// Retrieves a user by their login (username). Returns null if not found.
        /// </summary>
        public User? GetByLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return null;
            return Find(u => u.Login == login).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a user by their email address. Returns null if not found.
        /// </summary>
        public User? GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return Find(u => u.Email == email).FirstOrDefault();
        }
    }
}
