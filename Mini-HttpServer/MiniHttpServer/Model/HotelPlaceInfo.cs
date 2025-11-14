using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model
{
    public class HotelPlaceInfo
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string DistanceToAirport { get; set; } = string.Empty;
        public string DistanceToCenter { get; set; } = string.Empty;
        public string DistanceToBeach { get; set; } = string.Empty;
    }
}
