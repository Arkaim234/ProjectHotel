using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Settings;
using MiniHttpServer.Frimework.Core.HttpResponse;

using MiniHttpServer.Repositories;
using MiniHttpServer.Services;
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

            _hotelRepo = new HotelRepository(context);

            var roomRepo = new RoomTypeRepository(context);
            var mealRepo = new MealPlanRepository(context);        
            var descRepo = new HotelDescriptionRepository(context);
            var placeRepo = new HotelPlaceInfoRepository(context);
            var serviceRepo = new HotelServiceRepository(context);

            _hotelService = new HotelService(
                _hotelRepo,
                roomRepo,
                mealRepo,
                descRepo,
                placeRepo,
                serviceRepo
            );
        }

        [HttpGet("hotels")]
        public IActionResult HotelListPage()
        {
            var hotels = _hotelRepo.GetAllWithMealPlans();

            var model = new
            {
                Title = "Список отелей",
                Hotels = hotels
            };

            return new PageResult("Template/Page/hotels.thtml", model);
        }

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
    }
}
