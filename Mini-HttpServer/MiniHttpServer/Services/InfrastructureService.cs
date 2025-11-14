using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class InfrastructureService
    {
        private readonly HotelServiceRepository _repo;

        public InfrastructureService(HotelServiceRepository repo)
        {
            _repo = repo;
        }

        public Dictionary<string, List<string>> GetServiceMap(int hotelId)
        {
            return _repo.GetByHotelId(hotelId)
                .GroupBy(x => x.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => s.Name).ToList()
                );
        }
    }
}
