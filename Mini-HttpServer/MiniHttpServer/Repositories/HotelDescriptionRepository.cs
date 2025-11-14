using MiniHttpServer.Model;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Repositories
{
    public class HotelDescriptionRepository : OrmRepositories<HotelDescription>
    {
        public HotelDescriptionRepository(IORMContext context)
            : base(context, "HotelDescriptions") { }

        public HotelDescription? GetByHotelId(int hotelId)
        {
            return Find(x => x.HotelId == hotelId).FirstOrDefault();
        }
    }
}
