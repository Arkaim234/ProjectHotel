using System.Collections.Generic;
using System.Linq;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class HotelMealPlan
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int MealPlanId { get; set; }
    }

    public class HotelMealPlanRepository : OrmRepositories<HotelMealPlan>
    {
        public HotelMealPlanRepository(IORMContext context)
            : base(context, "hotel_mealplans")
        {
        }

        public HotelMealPlanRepository(string connectionString)
            : base(connectionString, "hotel_mealplans")
        {
        }

        /// <summary>
        /// Вернуть список ID типов питания, связанных с отелем.
        /// </summary>
        public List<int> GetMealPlanIdsForHotel(int hotelId)
        {
            return Find(x => x.HotelId == hotelId)
                .Select(x => x.MealPlanId)
                .ToList();
        }

        /// <summary>
        /// Вернуть список hotelId, у которых есть указанный MealPlanId.
        /// </summary>
        public List<int> GetHotelIdsByMealPlan(int mealPlanId)
        {
            return Find(x => x.MealPlanId == mealPlanId)
                .Select(x => x.HotelId)
                .ToList();
        }

        /// <summary>
        /// Добавить связь Hotel → MealPlan.
        /// </summary>
        public void AddMealPlan(int hotelId, int mealPlanId)
        {
            Add(new HotelMealPlan
            {
                HotelId = hotelId,
                MealPlanId = mealPlanId
            });
        }

        /// <summary>
        /// Удалить все связи для отеля.
        /// </summary>
        public void RemoveAllForHotel(int hotelId)
        {
            var items = Find(x => x.HotelId == hotelId).ToList();
            foreach (var row in items)
                Delete(row.Id);
        }
    }
}
