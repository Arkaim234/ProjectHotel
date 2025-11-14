using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model
{
    public class RoomType
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        public string Name { get; set; } = string.Empty; 
        public string View { get; set; } = string.Empty; 
        public string BedConfiguration { get; set; } = string.Empty;

        public int MaxOccupancy { get; set; }
        public int AreaSquareMeters { get; set; }
    }
}
