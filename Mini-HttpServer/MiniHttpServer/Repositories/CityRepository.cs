using MiniHttpServer.Model;
using MyORMLibrary;

namespace MiniHttpServer.Repositories
{
    public class CityRepository : OrmRepositories<City>
    {
        public CityRepository(IORMContext context)
            : base(context, "Cities") { }

        public IEnumerable<City> GetByCountry(int countryId)
        {
            return Find(c => c.CountryId == countryId);
        }
    }
}
