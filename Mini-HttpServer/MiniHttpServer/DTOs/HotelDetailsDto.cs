using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniHttpServer.DTOs.HelperDTOs;

namespace MiniHttpServer.DTOs
{
    public class HotelDetailsDto
    {
        // Наименование отеля
        public string Name { get; set; } = string.Empty;
        // Фото отеля
        public string PhotoUrl { get; set; } = string.Empty;

        // Расположение
        public List<InformatioPlaceHotelDto>? InformatioPlaceHotel { get; set; }
        // Пляж
        public bool HasBeach { get; set; }
        public string BeachDescription { get; set; } = string.Empty;
        // Основная информация       
        public List<HotelDescriptionDto>? HotelDescription { get; set; }
        // Контакты
        public List<ContactInfoDto> ContactsAndPay { get; set; } = new();
        // Примечание
        public string Notice { get; set; } = string.Empty;

        // В номере
        public List<string> InRoomAmenities { get; set; } = new();
        // Описание номеров
        public List<RoomTypeDto> RoomTypes { get; set; } = new();


        // Питание
        public List<MealPlanDto> AvailableMealPlans { get; set; } = new();
        // Для детей
        public List<string> ChildServices { get; set; } = new();
        // Развлечения и спорт
        public List<string> FreeEntertainment { get; set; } = new();
        public List<string> PaidEntertainment { get; set; } = new();
        // Услуги на территории
        public List<string> OnSiteServices { get; set; } = new(); 
    }
}
