using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.DTOs.HelperDTOs
{
    public class RoomTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string View { get; set; } = string.Empty;
        public string BedConfiguration { get; set; } = string.Empty; 
        public List<string>? Facilities { get; set; }  
        public int MaxOccupancy { get; set; }
        public int AreaSquareMeters { get; set; }
    }
}
