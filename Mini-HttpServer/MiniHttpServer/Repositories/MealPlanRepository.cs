using MiniHttpServer.Model;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Repositories
{
    public class MealPlanRepository : OrmRepositories<MealPlan>
    {
        public MealPlanRepository(IORMContext context)
            : base(context, "MealPlans") { }

        public IEnumerable<MealPlan> GetByHotel(int hotelId)
        {
            return Find(x => x.HotelId == hotelId);
        }
    }
}
