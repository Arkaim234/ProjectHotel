using System.Collections.Generic;
using MiniHttpServer.Model;
using MiniHttpServer.DTOs.HelperDTOs;  

namespace MiniHttpServer.DTOs
{
    public class HotelDetailsDto
    {
        public Hotel Hotel { get; set; } = null!;

        public HotelDescription? Description { get; set; }
        public HotelPlaceInfo? PlaceInfo { get; set; }

        public List<RoomType> RoomTypes { get; set; } = new();

        public List<MealPlanDto> AvailableMealPlans { get; set; } = new();

        public List<string> ChildServices { get; set; } = new();
        public List<string> FreeEntertainment { get; set; } = new();
        public List<string> PaidEntertainment { get; set; } = new();
        public List<string> OnSiteServices { get; set; } = new();

        public List<string> Contacts { get; set; } = new();
    }
}
