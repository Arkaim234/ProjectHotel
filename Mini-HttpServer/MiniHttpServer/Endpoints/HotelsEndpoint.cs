using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Settings;
using MiniHttpServer.Frimework.Core.HttpResponse;

using MiniHttpServer.Repositories;
using MiniHttpServer.Services;
using MiniHttpServer.Model.Filters;
using MyORMLibrary;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    public class HotelsEndpoint : EndpointBase
    {
        private readonly HotelRepository _hotelRepo;
        private readonly HotelService _hotelService;

        public HotelsEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);

            // Репозитории
            _hotelRepo = new HotelRepository(context);

            var roomRepo = new RoomTypeRepository(context);
            var mealRepo = new MealPlanRepository(context);
            var descRepo = new HotelDescriptionRepository(context);
            var placeRepo = new HotelPlaceInfoRepository(context);
            var serviceRepo = new HotelServiceRepository(context);

            // Сервис
            _hotelService = new HotelService(
                _hotelRepo,
                roomRepo,
                mealRepo,
                descRepo,
                placeRepo,
                serviceRepo
            );
        }

        // ---------------------------------------------------------
        // 1) Главная страница списка отелей (рендер thtml)
        // ---------------------------------------------------------
        [HttpGet("hotels")]
        public IActionResult HotelListPage()
        {
            var hotels = _hotelRepo.GetAll();

            var model = new
            {
                Title = "Список отелей",
                Hotels = hotels
            };

            return new PageResult("Template/Page/hotels.thtml", model);
        }

        // ---------------------------------------------------------
        // 2) Страница конкретного отеля
        // ---------------------------------------------------------
        [HttpGet("hotels/{slug}")]
        public IActionResult GetHotelPage(string slug)
        {
            var dto = _hotelService.GetHotelDetails(slug);
            if (dto == null)
            {
                Context.Response.StatusCode = 404;
                return Json(new { message = "Hotel not found" });
            }

            return new PageResult("Template/Page/hotel-details.thtml", dto);
        }

        // ---------------------------------------------------------
        // 3) AJAX-фильтр
        // ---------------------------------------------------------
        [HttpPost("hotels/search")]
        public IActionResult SearchHotels(HotelFilter filter)
        {
            var hotels = _hotelRepo.Search(filter);

            return new JsonResult(new
            {
                success = true,
                count = hotels.Count(),
                items = hotels
            });
        }

        // ---------------------------------------------------------
        // 4) AJAX фильтр по типу
        // ---------------------------------------------------------
        [HttpGet("hotels/type/{type}")]
        public IActionResult SearchByType(string type)
        {
            var hotels = _hotelRepo.SearchByType(type);

            return new JsonResult(new
            {
                success = true,
                count = hotels.Count(),
                items = hotels
            });
        }
    }
}
