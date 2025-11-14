using MiniHttpServer.Model;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Repositories
{
    public class HotelPlaceInfoRepository : OrmRepositories<HotelPlaceInfo>
    {
        public HotelPlaceInfoRepository(IORMContext context)
            : base(context, "HotelPlaceInfos") { }

        public HotelPlaceInfo? GetByHotelId(int hotelId)
        {
            return Find(x => x.HotelId == hotelId).FirstOrDefault();
        }
    }
}
