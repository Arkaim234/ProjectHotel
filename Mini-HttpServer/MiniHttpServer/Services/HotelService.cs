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
            _mealRepo = mealRepo;          
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

            // типы питания с описаниями
            var mealPlanDtos = new List<MealPlanDto>();
            if (hotel.MealPlans != null && hotel.MealPlans.Any())
            {
                var codes = hotel.MealPlans;
                var meals = _mealRepo.Find(m => codes.Contains(m.Code)).ToList();

                mealPlanDtos = meals
                    .Select(m => new MealPlanDto
                    {
                        Code = m.Code,
                        Description = m.Description
                    })
                    .ToList();
            }

            // ФОТО: берём либо одну картинку, либо все из директории, указанной в Hotel.PhotoUrl
            var (photos, coverPhoto) = HotelPhotosHelper.ResolvePhotos(hotel);
            var coverUrl = coverPhoto ?? "/images/default-hotel.jpg"; 

            var hotelDto = new HotelDetailsDto
            {
                Hotel = hotel,
                Description = description,
                PlaceInfo = placeInfo,
                RoomTypes = roomTypes,
                AvailableMealPlans = mealPlanDtos,
                Contacts = hotel.Contacts != null ? hotel.Contacts.ToList() : new List<string>(),
                PhotoUrls = photos,
                CoverPhotoUrl = coverUrl
            };

            // В НОМЕРЕ
            hotelDto.InRoomServices = services
                .Where(s => s.Category == "В номере")
                .Select(s => s.Name)
                .ToList();

            // ДЛЯ ДЕТЕЙ
            hotelDto.ChildServices = services
                .Where(s => s.Category == "Для детей")
                .Select(s => s.Name)
                .ToList();

            var entertainmentNames = services
                .Where(s => s.Category != null && s.Category.Trim() == "Развлечения и спорт")
                .Select(s => s.Name ?? string.Empty)
                .ToList();

            hotelDto.PaidEntertainment = entertainmentNames
                .Where(name => name.ToLower().Contains("платно"))
                .ToList();

            hotelDto.FreeEntertainment = entertainmentNames
                .Where(name => !name.ToLower().Contains("платно"))
                .ToList();

            // УСЛУГИ НА ТЕРРИТОРИИ
            hotelDto.OnSiteServices = services
                .Where(s => s.Category == "Услуги на территории")
                .Select(s => s.Name)
                .ToList();

            hotelDto.FreeEntertainmentHtml = BuildListHtml(hotelDto.FreeEntertainment);
            hotelDto.PaidEntertainmentHtml = BuildListHtml(hotelDto.PaidEntertainment);

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

        private static string BuildListHtml(IEnumerable<string> items)
        {
            if (items == null)
                return string.Empty;

            var arr = items
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (arr.Length == 0)
                return string.Empty;

            // <li>• текст</li><li>• текст2</li>...
            return string.Join("", arr.Select(s => $"<li>• {s}</li>"));
        }
    }
}
