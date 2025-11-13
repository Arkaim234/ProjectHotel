using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.DTOs.HelperDTOs
{
    public class HotelDescriptionDto
    {
        public int YearOpened { get; set; }
        public int YearRenovated { get; set; }
        public decimal TotalAreaSquareMeters { get; set; }
        public string BuildingInfo { get; set; } = string.Empty;
    }
}
