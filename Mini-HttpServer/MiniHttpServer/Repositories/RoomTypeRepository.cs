using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniHttpServer.Model;

namespace MiniHttpServer.Repositories
{
    public class RoomTypeRepository : OrmRepositories<RoomType>
    {
        public RoomTypeRepository(IORMContext context)
            : base(context, "RoomTypes") { }

        public IEnumerable<RoomType> GetByHotel(int hotelId)
        {
            return Find(x => x.HotelId == hotelId);
        }
    }
}
