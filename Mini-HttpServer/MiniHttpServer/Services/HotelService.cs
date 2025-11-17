using System.Collections.Generic;
using System.Linq;
using MiniHttpServer.DTOs;
using MiniHttpServer.DTOs.HelperDTOs;
using MiniHttpServer.Model;
using MiniHttpServer.Model.Filters;
using MiniHttpServer.Repositories;

namespace MiniHttpServer.Services
{
    public class HotelService
    {
        private readonly HotelRepository _hotelRepo;
        private readonly RoomTypeRepository _roomRepo;
        private readonly MealPlanRepository _mealRepo;
        private readonly HotelDescriptionRepository _descRepo;
        private readonly HotelPlaceInfoRepository _placeRepo;
        private readonly HotelServiceRepository _serviceRepo;

        public HotelService(
            HotelRepository hotelRepo,
            RoomTypeRepository roomRepo,
            MealPlanRepository mealRepo,
            HotelDescriptionRepository descRepo,
            HotelPlaceInfoRepository placeRepo,
            HotelServiceRepository serviceRepo)
        {
            _hotelRepo = hotelRepo;
            _roomRepo = roomRepo;
            _mealRepo = mealRepo;          // ВЕРНУЛИ
            _descRepo = descRepo;
            _placeRepo = placeRepo;
            _serviceRepo = serviceRepo;
        }

        // ---------- Детальная страница ----------
        public HotelDetailsDto? GetHotelDetails(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            var hotel = _hotelRepo.GetBySlug(slug);
            if (hotel == null)
                return null;

            var description = _descRepo.GetByHotelId(hotel.Id);
            var placeInfo = _placeRepo.GetByHotelId(hotel.Id);
            var roomTypes = _roomRepo.GetByHotel(hotel.Id).ToList();
            var services = _serviceRepo.GetByHotelId(hotel.Id).ToList();

            // --- типы питания: код + описание ---
            var mealPlanDtos = new List<MealPlanDto>();

            if (hotel.MealPlans != null && hotel.MealPlans.Any())
            {
                var codes = hotel.MealPlans; // List<string> кодов (BB, AI, ...)

                var meals = _mealRepo
                    .Find(m => codes.Contains(m.Code))
                    .ToList();

                mealPlanDtos = meals
                    .Select(m => new MealPlanDto
                    {
                        Code = m.Code,
                        Description = m.Description
                    })
                    .ToList();
            }

            var hotelDto = new HotelDetailsDto
            {
                Hotel = hotel,
                Description = description,
                PlaceInfo = placeInfo,
                RoomTypes = roomTypes,
                AvailableMealPlans = mealPlanDtos,

                Contacts = hotel.Contacts != null
                    ? hotel.Contacts.ToList()
                    : new List<string>()
            };

            // Сервисы
            hotelDto.ChildServices = services
                .Where(s => s.Category == "Для детей")
                .Select(s => s.Name)
                .ToList();

            hotelDto.FreeEntertainment = services
                .Where(s => s.Category == "Развлечения и спорт" && !s.Name.Contains("(платно)"))
                .Select(s => s.Name)
                .ToList();

            hotelDto.PaidEntertainment = services
                .Where(s => s.Category == "Развлечения и спорт" && s.Name.Contains("(платно)"))
                .Select(s => s.Name)
                .ToList();

            hotelDto.OnSiteServices = services
                .Where(s => s.Category == "Услуги на территории")
                .Select(s => s.Name)
                .ToList();

            return hotelDto;
        }

        // ---------- Поиск для фильтров ----------
        public IEnumerable<Hotel> SearchHotels(int cityId, string? mealPlanCode)
        {
            var filter = new HotelFilter
            {
                CityId = cityId,
                MealPlanCode = mealPlanCode
            };

            return _hotelRepo.Search(filter);
        }

        public IEnumerable<Hotel> GetAll() => _hotelRepo.GetAllWithMealPlans();
    }
}
