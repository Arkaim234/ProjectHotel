using MiniHttpServer.Model;
using MyORMLibrary;
using System.Collections.Generic;
using System.Linq;

namespace MiniHttpServer.Repositories
{
    public class MealPlanRepository : OrmRepositories<MealPlan>
    {
        public MealPlanRepository(IORMContext context)
            : base(context, "mealplans")
        {
        }

        public MealPlanRepository(string connectionString)
            : base(connectionString, "mealplans")
        {
        }

        /// <summary>
        /// Вернуть ВСЕ коды питания (AI, BB, FB...)
        /// </summary>
        public IEnumerable<string> GetAllCodes()
        {
            return GetAll()
                .Select(mp => mp.Code)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c);
        }
    }
}
