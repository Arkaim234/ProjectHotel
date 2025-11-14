using MiniHttpServer.DTOs;
using MiniHttpServer.DTOs.HelperDTOs;
using MiniHttpServer.Model;
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
            _mealRepo = mealRepo;
            _descRepo = descRepo;
            _placeRepo = placeRepo;
            _serviceRepo = serviceRepo;
        }

        // Детальная страница отеля
        public HotelDetailsDto? GetHotelDetails(string slug)
        {
            var hotel = _hotelRepo.GetBySlug(slug);
            if (hotel == null) return null;

            // Конвертация в DTO
            var hotelDto = new HotelDetailsDto
            {
                Hotel = hotel,
                Description = ConvertDescription(_descRepo.GetByHotelId(hotel.Id)),
                PlaceInfo = ConvertPlace(_placeRepo.GetByHotelId(hotel.Id)),
                RoomTypes = ConvertRooms(_roomRepo.GetByHotel(hotel.Id)),
                AvailableMealPlans = ConvertMeals(_mealRepo.GetByHotel(hotel.Id))
            };

            // Услуги
            var services = _serviceRepo.GetByHotelId(hotel.Id).ToList();

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


        // ---------- Конвертация ----------
        private HotelDescriptionDto? ConvertDescription(HotelDescription? d)
        {
            if (d == null) return null;

            return new HotelDescriptionDto
            {
                YearOpened = d.YearOpened,
                YearRenovated = d.YearRenovated,
                TotalAreaSquareMeters = d.TotalAreaSquareMeters,
                BuildingInfo = d.BuildingInfo
            };
        }

        private InformatioPlaceHotelDto? ConvertPlace(HotelPlaceInfo? p)
        {
            if (p == null) return null;

            return new InformatioPlaceHotelDto
            {
                Address = p.Address,
                City = p.City,
                Country = p.Country,
                DistanceToAirport = p.DistanceToAirport,
                DistanceToCenter = p.DistanceToCenter,
                DistanceToBeach = p.DistanceToBeach
            };
        }

        private List<RoomTypeDto> ConvertRooms(IEnumerable<RoomType> rooms)
        {
            return rooms.Select(r => new RoomTypeDto
            {
                Name = r.Name,
                View = r.View,
                BedConfiguration = r.BedConfiguration,
                MaxOccupancy = r.MaxOccupancy,
                AreaSquareMeters = r.AreaSquareMeters
            }).ToList();
        }

        private List<MealPlanDto> ConvertMeals(IEnumerable<MealPlan> meals)
        {
            return meals.Select(m => new MealPlanDto
            {
                Code = m.Code,
                Description = m.Description
            }).ToList();
        }


        // ---------- Поиск ----------
        public IEnumerable<Hotel> SearchHotels(int cityId, string? mealPlan)
        {
            return _hotelRepo.Search(cityId, mealPlan);
        }

        public IEnumerable<Hotel> GetAll() => _hotelRepo.GetAll();
    }
}
