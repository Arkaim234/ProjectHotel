using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model
{
    public class MealPlan
    {
        public int Id { get; set; }
        public int HotelId { get; set; }

        public string Code { get; set; } = string.Empty;       
        public string Description { get; set; } = string.Empty;

        public bool SmokingAllowedInRestaurant { get; set; }
    }
}
