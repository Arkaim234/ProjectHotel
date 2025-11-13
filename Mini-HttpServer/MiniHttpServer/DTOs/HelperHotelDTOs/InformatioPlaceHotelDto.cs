using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.DTOs.HelperDTOs
{
    public class InformatioPlaceHotelDto
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string DistanceToAirport { get; set; } = string.Empty;
        public List<string> NearbyHotels { get; set; } = new();
        public List<string> TimeDistanceToLandmarks { get; set; } = new();
    }
}
