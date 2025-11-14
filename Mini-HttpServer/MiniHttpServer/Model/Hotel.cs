using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Slug { get; set; }
        public int CityId { get; set; }
        public string HotelType { get; set; } 
        public string Description { get; set; } 
        public string PhotoUrl { get; set; } 
        public string? MealPlanCode { get; set; }
        public int Price { get; set; }
    }
}
