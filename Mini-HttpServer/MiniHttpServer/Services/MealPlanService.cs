using MiniHttpServer.Model;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class MealPlanService
    {
        private readonly HotelMealPlanRepository _hmpRepo;
        private readonly MealPlanRepository _mealRepo;

        public MealPlanService(
            HotelMealPlanRepository hmpRepo,
            MealPlanRepository mealRepo)
        {
            _hmpRepo = hmpRepo;
            _mealRepo = mealRepo;
        }

        // Получить планы питания отеля
        public IEnumerable<MealPlan> GetHotelMeals(int hotelId)
        {
            var mealIds = _hmpRepo.GetMealPlanIdsForHotel(hotelId);

            return _mealRepo
                .Find(m => mealIds.Contains(m.Id))
                .ToList();
        }
    }
}
