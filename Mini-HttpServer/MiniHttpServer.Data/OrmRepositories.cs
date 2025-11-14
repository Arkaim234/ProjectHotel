using System.Linq.Expressions;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class OrmRepositories<T> : IRepository<T> where T : class, new()
    {
        private readonly IORMContext _context;
        private readonly string _tableName;

        public OrmRepositories(IORMContext context, string tableName)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        // Упрощённый конструктор: принимает только строку подключения.
        public OrmRepositories(string connectionString, string tableName)
            : this(new ORMContext(connectionString), tableName) { }

        public IEnumerable<T> GetAll() => _context.ReadByAll<T>(_tableName);
        public T? GetById(int id) => _context.ReadById<T>(id, _tableName);
        public T Add(T entity) => _context.Create(entity, _tableName);
        public void Update(int id, T entity) => _context.Update(id, entity, _tableName);
        public void Delete(int id) => _context.Delete(id, _tableName);
        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate) => _context.Where(predicate, _tableName);
    }
}
