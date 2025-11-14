using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Model.Filters
{
    /// <summary>
    /// Критерии поиска отелей. Все свойства необязательны.
    /// </summary>
    public class HotelFilter
    {
        public string? Type { get; set; }   
        public int? CityId { get; set; }    
        public string? Name { get; set; }    
        public string? MealPlanCode { get; set; }
    }
}