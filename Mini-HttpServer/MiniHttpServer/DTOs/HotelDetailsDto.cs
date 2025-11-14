using System.Collections.Generic;
using MiniHttpServer.DTOs.HelperDTOs;
using MiniHttpServer.Model;

namespace MiniHttpServer.DTOs
{
    public class HotelDetailsDto
    {
        // Базовая инфа об отеле (имя, фото, описание, цена и т.п.)
        public Hotel Hotel { get; set; } = null!;

        // Описание (год открытия, реновации, площадь, корпус)
        public HotelDescriptionDto? Description { get; set; }

        // Расположение, расстояния, окрестности
        public InformatioPlaceHotelDto? PlaceInfo { get; set; }

        // Номера
        public List<RoomTypeDto> RoomTypes { get; set; } = new();

        // Питание
        public List<MealPlanDto> AvailableMealPlans { get; set; } = new();

        // Для детей
        public List<string> ChildServices { get; set; } = new();
        // Развлечения и спорт (бесплатно)
        public List<string> FreeEntertainment { get; set; } = new();
        // Развлечения и спорт (платно)
        public List<string> PaidEntertainment { get; set; } = new();
        // Услуги на территории
        public List<string> OnSiteServices { get; set; } = new();

        // Контакты (сайт, телефон, почта) — если захочешь использовать
        public List<ContactInfoDto> Contacts { get; set; } = new();
    }
}
