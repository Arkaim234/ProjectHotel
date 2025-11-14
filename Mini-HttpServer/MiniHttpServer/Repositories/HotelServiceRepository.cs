using MiniHttpServer.Model;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Repositories
{
    public class HotelServiceRepository : OrmRepositories<HotelService>
    {
        public HotelServiceRepository(IORMContext context)
            : base(context, "HotelServices") { }

        public IEnumerable<HotelService> GetByHotelId(int hotelId)
        {
            return Find(s => s.HotelId == hotelId);
        }

        public IEnumerable<HotelService> GetByCategory(int hotelId, string category)
        {
            return Find(s => s.HotelId == hotelId && s.Category == category);
        }
    }
}
