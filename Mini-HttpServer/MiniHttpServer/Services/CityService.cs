using MiniHttpServer.Model;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class CityService
    {
        private readonly CityRepository _cities;

        public CityService(CityRepository cities)
        {
            _cities = cities;
        }

        public IEnumerable<City> GetCitiesByCountry(int countryId)
        {
            return _cities.GetByCountry(countryId);
        }
    }
}