using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.DTOs.HelperDTOs
{
    public class MealPlanDto
    {
        public string Code { get; set; } = string.Empty;     
        public string Description { get; set; } = string.Empty;
        public List<string> Restaurants { get; set; } = new();
        public bool SmokingAllowedInRestaurant { get; set; }
    }
}
