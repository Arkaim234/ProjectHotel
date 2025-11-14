using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MiniHttpServer.Model;
using MiniHttpServer.Model.Filters;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    /// <summary>
    /// Репозиторий для работы с отелями.
    /// </summary>
    public class HotelRepository : OrmRepositories<Hotel>
    {
        public HotelRepository(IORMContext context)
            : base(context, "Hotels") { }

        public HotelRepository(string connectionString)
            : base(connectionString, "Hotels") { }

        /// <summary>
        /// Получить отель по его slug (для страницы отеля).
        /// </summary>
        public Hotel? GetBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            return Find(h => h.Slug == slug).FirstOrDefault();
        }

        /// <summary>
        /// Поиск по типу отеля (HotelType).
        /// Если type пустой — возвращаем все отели.
        /// </summary>
        public IEnumerable<Hotel> SearchByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return GetAll();

            return Find(h => h.HotelType == type);
        }

        /// <summary>
        /// Поиск по имени отеля (LIKE '%name%').
        /// Если name пустой — возвращаем все.
        /// </summary>
        public IEnumerable<Hotel> SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return GetAll();

            // .Contains будет ORM переведён в ILIKE '%value%' для PostgreSQL
            return Find(h => h.Name.Contains(name));
        }

        /// <summary>
        /// Комплексный поиск по фильтру.
        /// Вся фильтрация уходит в базу (через Expression → SQL).
        /// </summary>
        public IEnumerable<Hotel> Search(HotelFilter filter)
        {
            if (filter == null)
                return GetAll();

            Expression<Func<Hotel, bool>> predicate = h => true;

            if (!string.IsNullOrWhiteSpace(filter.Name))
                predicate = And(predicate, h => h.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Type))
                predicate = And(predicate, h => h.HotelType == filter.Type);

            if (!string.IsNullOrWhiteSpace(filter.MealPlanCode))
                predicate = And(predicate, h => h.MealPlanCode == filter.MealPlanCode);

            if (filter.CityId != null)
                predicate = And(predicate, h => h.CityId == filter.CityId);

            return Find(predicate);
        }
        public IEnumerable<Hotel> Search(int cityId, string? mealPlan)
        {
            var filter = new HotelFilter
            {
                CityId = cityId,
                MealPlanCode = mealPlan
            };

            return Search(filter);
        }

        /// <summary>
        /// Вернуть список всех уникальных типов отелей (HotelType).
        /// </summary>
        public IEnumerable<string> GetAllHotelTypes()
        {
            return GetAll()
                .Select(h => h.HotelType)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t);
        }

        /// <summary>
        /// Помогалка для склейки нескольких выражений через AND.
        /// </summary>
        private Expression<Func<T, bool>> And<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var param = Expression.Parameter(typeof(T), "h");

            var body = Expression.AndAlso(
                Expression.Invoke(expr1, param),
                Expression.Invoke(expr2, param)
            );

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
