using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyORMLibrary
{
    public interface IORMContext
    {
        T? ReadById<T>(int id, string tableName) where T : class, new();
        List<T> ReadByAll<T>(string tableName) where T : class, new();
        T Create<T>(T entity, string tableName) where T : class;
        void Update<T>(int id, T entity, string tableName) where T : class;
        void Delete(int id, string tableName);
        IEnumerable<T> Where<T>(Expression<Func<T, bool>> predicate, string tableName) where T : class, new();
        T? FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string tableName) where T : class, new();
    }
}
