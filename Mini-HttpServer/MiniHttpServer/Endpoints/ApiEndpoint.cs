using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Frimework.Core.HttpResponse;
using MiniHttpServer.Frimework.Settings;
using MiniHttpServer.Services;
using MiniHttpServer.Repositories;
using MiniHttpServer.Model.Filters;
using MyORMLibrary;

namespace MiniHttpServer.Endpoints
{
    /// <summary>
    /// Вспомогательный API для фильтров (города, типы отелей, питания и т.п.).
    /// УРЛы: /api/...
    /// </summary>
    [Endpoint]
    internal class ApiEndpoint : EndpointBase
    {
        private readonly HotelRepository _hotelRepo;
        private readonly CityRepository _cityRepo;
        private readonly MealPlanRepository _mealRepo;
        private readonly HotelCategoryMapRepository _hotelCategoryMapRepo;

        public ApiEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);

            _hotelRepo = new HotelRepository(context);
            _cityRepo = new CityRepository(context);
            _mealRepo = new MealPlanRepository(context);
            _hotelCategoryMapRepo = new HotelCategoryMapRepository(context);
        }

        // --------- Города ---------

        // GET /api/cities
        [HttpGet("cities")]
        public IActionResult GetCities()
        {
            var cities = _cityRepo
                .GetAll()
                .Select(c => new { id = c.Id, name = c.Name })
                .OrderBy(c => c.name)
                .ToList();

            return new JsonResult(cities);
        }

        // GET /api/cities/russia
        [HttpGet("cities/russia")]
        public IActionResult GetRussianCities()
        {
            var cities = _cityRepo
                .GetAll()
                .Where(c => c.CountryId == 1)        // 1 — Россия
                .Select(c => new { id = c.Id, name = c.Name })
                .OrderBy(c => c.name)
                .ToList();

            return new JsonResult(cities);
        }

        // *** НОВЫЙ МЕТОД ***
        // GET /api/cities/by-country?countryId=2
        [HttpGet("cities/by-country")]
        public IActionResult GetCitiesByCountry(int countryId)
        {
            // безопасный вариант через GetAll()
            var cities = _cityRepo
                .GetAll()
                .Where(c => c.CountryId == countryId)
                .Select(c => new { id = c.Id, name = c.Name })
                .OrderBy(c => c.name)
                .ToList();

            return new JsonResult(cities);
        }

        // GET /api/cities/search?q=mos
        [HttpGet("cities/search")]
        public IActionResult SearchCities(string q)
        {
            q = q?.Trim() ?? "";

            var cities = string.IsNullOrWhiteSpace(q)
                ? _cityRepo.GetAll()
                : _cityRepo.GetAll()
                           .Where(c => c.Name.ToLower().Contains(q.ToLower()));

            var result = cities
                .Where(c => c.CountryId == 1) // только Россия
                .Select(c => new { id = c.Id, name = c.Name })
                .OrderBy(c => c.name)
                .ToList();

            return new JsonResult(result);
        }

        // --------- Страны ---------

        // GET /api/countries
        [HttpGet("countries")]
        public IActionResult GetCountries()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);
            var repo = new CountryRepository(context);

            var data = repo.GetAll()
                           .Select(c => new { id = c.Id, name = c.Name })
                           .OrderBy(c => c.name)
                           .ToList();

            return new JsonResult(data);
        }

        // GET /api/countries/search?q=tu
        [HttpGet("countries/search")]
        public IActionResult SearchCountries(string q)
        {
            q = q?.Trim() ?? "";

            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);
            var repo = new CountryRepository(context);

            var all = repo.GetAll();

            var filtered = string.IsNullOrWhiteSpace(q)
                ? all
                : all.Where(c => c.Name.ToLower().Contains(q.ToLower()));

            var result = filtered
                .Where(c => c.Id != 1) // исключаем Россию
                .Select(c => new { id = c.Id, name = c.Name })
                .OrderBy(c => c.name)
                .ToList();

            return new JsonResult(result);
        }

        // --------- Категории (типы отелей) ---------

        // GET /api/hoteltypes
        [HttpGet("hoteltypes")]
        public IActionResult GetHotelTypes()
        {
            // filters.js ждёт массив строк
            var types = _hotelRepo
                .GetAllHotelTypes()
                .ToList();

            return new JsonResult(types);
        }

        // /api/categories
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);
            var repo = new HotelCategoryRepository(context);

            var data = repo.GetAll()
                           .Select(c => new { id = c.Id, name = c.Name })
                           .OrderBy(c => c.name)
                           .ToList();

            return new JsonResult(data);
        }

        // --------- Типы питания ---------

        // GET /api/mealplans
        [HttpGet("mealplans")]
        public IActionResult GetMealPlans()
        {
            var plans = _mealRepo
                .GetAll()
                .Select(m => new
                {
                    id = m.Id,
                    code = m.Code
                })
                .OrderBy(m => m.code)
                .ToList();

            return new JsonResult(plans);
        }

        // --------- Список отелей (для нижнего фильтра "Отели") ---------

        // GET /api/hotels/all
        [HttpGet("hotels/all")]
        public IActionResult GetHotelsList()
        {
            var hotels = _hotelRepo
                .GetAll()
                .Select(h => new { id = h.Id, name = h.Name })
                .OrderBy(h => h.name)
                .ToList();

            return new JsonResult(hotels);
        }

        // --------- ПОИСК ТУРОВ / ОТЕЛЕЙ ДЛЯ КНОПКИ "НАЙТИ" ---------
        [HttpGet("hotels/search")]
        public IActionResult SearchHotels(
            int? fromCityId,
            int? countryId,
            string? dateFrom,
            string? dateTo,
            int? nightsFrom,
            int? nightsTo,
            int? adults,
            int? children,
            string? cityIds,
            string? categoryIds,
            string? hotelIds,
            string? mealCodes
        )
        {
            // Берём все отели сразу с типами питания
            var hotels = _hotelRepo.GetAllWithMealPlans().ToList();

            // Фильтр по стране назначения -> набор допустимых городов
            if (countryId.HasValue)
            {
                var countryCitySet = _cityRepo
                    .GetAll()
                    .Where(c => c.CountryId == countryId.Value)
                    .Select(c => c.Id)
                    .ToHashSet();

                hotels = hotels
                    .Where(h => countryCitySet.Contains(h.CityId))
                    .ToList();
            }

            // Нижний фильтр "Город" (список ID через запятую)
            if (!string.IsNullOrWhiteSpace(cityIds))
            {
                var cityIdList = cityIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (cityIdList.Any())
                {
                    var citySet = cityIdList.ToHashSet();
                    hotels = hotels
                        .Where(h => citySet.Contains(h.CityId))
                        .ToList();
                }
            }

            // Конкретные отели (чекбоксы "Отели")
            if (!string.IsNullOrWhiteSpace(hotelIds))
            {
                var hotelIdSet = hotelIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToHashSet();

                if (hotelIdSet.Any())
                {
                    hotels = hotels
                        .Where(h => hotelIdSet.Contains(h.Id))
                        .ToList();
                }
            }

            // Категории (звёздность и т.п.) 
            if (!string.IsNullOrWhiteSpace(categoryIds))
            {
                var categoryIdList = categoryIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var id) ? (int?)id : null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                if (categoryIdList.Any())
                {
                    var mappings = _hotelCategoryMapRepo
                        .Find(m => categoryIdList.Contains(m.CategoryId))
                        .ToList();

                    var allowedHotelIds = mappings
                        .Select(m => m.HotelId)
                        .ToHashSet();

                    hotels = hotels
                        .Where(h => allowedHotelIds.Contains(h.Id))
                        .ToList();
                }
            }

            // Питание (BB / AI / HB ...). Если выбран хотя бы 1 код — фильтруем
            if (!string.IsNullOrWhiteSpace(mealCodes))
            {
                var mealCodeList = mealCodes
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToList();

                if (mealCodeList.Any())
                {
                    var mealSet = mealCodeList.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    hotels = hotels
                        .Where(h => h.MealPlans != null &&
                                    h.MealPlans.Any(code => mealSet.Contains(code)))
                        .ToList();
                }
            }

            // Даты / ночи / количество человек пока игнорируем

            // Собираем DTO для фронта
            var allCities = _cityRepo.GetAll().ToList();
            var cityDict = allCities.ToDictionary(c => c.Id, c => c.Name);

            var result = hotels.Select(h =>
            {
                // Берём фотки и обложку так же, как на детальной странице
                var (photos, cover) = HotelPhotosHelper.ResolvePhotos(h);
                var coverUrl = cover ?? "/images/no-photo.png"; // заглушка, если фоток нет

                return new
                {
                    id = h.Id,
                    name = h.Name,
                    city = cityDict.TryGetValue(h.CityId, out var cityName) ? cityName : "",
                    price = h.Price,
                    slug = h.Slug,
                    photoUrl = coverUrl,   // сюда идёт уже готовый URL файла изображения
                    mealPlans = h.MealPlans
                };
            }).ToList();

            return new JsonResult(result);
        }
    }
}
