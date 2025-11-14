using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model
{
    public class HotelService
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
