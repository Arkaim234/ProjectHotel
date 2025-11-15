using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MiniHttpServer.Model;
using MiniHttpServer.Model.Filters;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class HotelRepository : OrmRepositories<Hotel>
    {
        private readonly HotelMealPlanRepository _hmpRepo;
        private readonly MealPlanRepository _mealRepo;

        public HotelRepository(IORMContext context)
            : base(context, "Hotels")
        {
            _hmpRepo = new HotelMealPlanRepository(context);
            _mealRepo = new MealPlanRepository(context);
        }

        public HotelRepository(string connectionString)
            : base(connectionString, "Hotels")
        {
            var ctx = new ORMContext(connectionString);
            _hmpRepo = new HotelMealPlanRepository(ctx);
            _mealRepo = new MealPlanRepository(ctx);
        }

        /* =====================================================
                         Получение одного отеля
        ====================================================== */
        public Hotel? GetBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            var hotel = Find(h => h.Slug == slug).FirstOrDefault();
            if (hotel == null)
                return null;

            hotel.MealPlans = LoadMealPlans(hotel.Id);
            return hotel;
        }

        /* =====================================================
                     Получить всех отелей + питание
        ====================================================== */
        public IEnumerable<Hotel> GetAllWithMealPlans()
        {
            var hotels = GetAll().ToList();

            foreach (var h in hotels)
                h.MealPlans = LoadMealPlans(h.Id);

            return hotels;
        }

        /* =====================================================
                     Загрузка типов питания (Many-To-Many)
        ====================================================== */
        private List<string> LoadMealPlans(int hotelId)
        {
            var mpIds = _hmpRepo.GetMealPlanIdsForHotel(hotelId);

            return _mealRepo
                .Find(m => mpIds.Contains(m.Id))
                .Select(m => m.Code)
                .ToList();
        }

        /* =====================================================
                             Поиск по типу
        ====================================================== */
        public IEnumerable<Hotel> SearchByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return GetAllWithMealPlans();

            var hotels = Find(h => h.HotelType == type).ToList();

            foreach (var h in hotels)
                h.MealPlans = LoadMealPlans(h.Id);

            return hotels;
        }

        /* =====================================================
                             Поиск по имени
        ====================================================== */
        public IEnumerable<Hotel> SearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return GetAllWithMealPlans();

            var hotels = Find(h => h.Name.Contains(name)).ToList();

            foreach (var h in hotels)
                h.MealPlans = LoadMealPlans(h.Id);

            return hotels;
        }

        /* =====================================================
                        Комплексный поиск
        ====================================================== */
        public IEnumerable<Hotel> Search(HotelFilter filter)
        {
            if (filter == null)
                return GetAllWithMealPlans();

            Expression<Func<Hotel, bool>> predicate = h => true;

            if (!string.IsNullOrWhiteSpace(filter.Name))
                predicate = And(predicate, h => h.Name.Contains(filter.Name));

            if (!string.IsNullOrWhiteSpace(filter.Type))
                predicate = And(predicate, h => h.HotelType == filter.Type);

            if (filter.CityId != null)
                predicate = And(predicate, h => h.CityId == filter.CityId);

            var hotels = Find(predicate).ToList();

            foreach (var h in hotels)
                h.MealPlans = LoadMealPlans(h.Id);

            return hotels;
        }

        /* =====================================================
                        Список категорий отелей
        ====================================================== */
        public IEnumerable<string> GetAllHotelTypes()
        {
            return GetAll()
                .Select(h => h.HotelType)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t);
        }

        /* =====================================================
                    Склейка выражений через AND
        ====================================================== */
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
