using MiniHttpServer.Model;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class MealPlanService
    {
        private readonly MealPlanRepository _repo;

        public MealPlanService(MealPlanRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<MealPlan> GetHotelMeals(int hotelId)
        {
            return _repo.GetByHotel(hotelId);
        }
    }
}
