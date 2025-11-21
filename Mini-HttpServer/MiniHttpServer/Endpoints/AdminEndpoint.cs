using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using MiniHttpServer.Frimework.Core;
using MiniHttpServer.Frimework.Core.Atributes;
using MiniHttpServer.Frimework.Core.HttpResponse;
using MiniHttpServer.Frimework.Settings;

using MiniHttpServer.Model;
using MiniHttpServer.Repositories;
using MiniHttpServer.Services;

// алиас, чтобы не путать с MiniHttpServer.Services.HotelService
using HotelServiceEntity = MiniHttpServer.Model.HotelService;

using MyORMLibrary;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AdminEndpoint : EndpointBase
    {
        private readonly HotelRepository _hotelRepository;
        private readonly HotelDescriptionRepository _hotelDescriptionRepository;
        private readonly HotelPlaceInfoRepository _hotelPlaceInfoRepository;
        private readonly HotelServiceRepository _hotelServiceRepository;
        private readonly RoomTypeRepository _roomTypeRepository;

        public AdminEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            var context = new ORMContext(settings.ConectionString);

            _hotelRepository = new HotelRepository(context);
            _hotelDescriptionRepository = new HotelDescriptionRepository(context);
            _hotelPlaceInfoRepository = new HotelPlaceInfoRepository(context);
            _hotelServiceRepository = new HotelServiceRepository(context);
            _roomTypeRepository = new RoomTypeRepository(context);
        }

        // ================== общая проверка прав ==================

        private (int userId, string role) RequireAdminOrEmployee()
        {
            var info = GetCurrentUser();
            if (info == null)
                throw new UnauthorizedAccessException("Не авторизован");

            var (userId, role) = info.Value;

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isEmployee)
                throw new UnauthorizedAccessException("Доступ запрещён");

            return (userId, role);
        }

        // ================== утилиты ==================

        private Dictionary<string, string> ReadForm()
        {
            var body = RequestBody ?? string.Empty;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(body))
                return dict;

            var pairs = body.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var kv = pair.Split('=', 2);

                var key = WebUtility.UrlDecode(kv[0] ?? string.Empty);
                var value = kv.Length > 1 ? WebUtility.UrlDecode(kv[1]) : string.Empty;

                if (!string.IsNullOrEmpty(key))
                    dict[key] = value;
            }

            return dict;
        }

        private static string BuildServicesLines(IEnumerable<HotelServiceEntity> services, string category)
        {
            if (services == null)
                return string.Empty;

            var items = services
                .Where(s => string.Equals(s.Category, category, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            if (items.Count == 0)
                return string.Empty;

            return string.Join("; ", items);
        }

        private static string[] SplitServiceItems(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            var normalized = text
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');

            var rawParts = normalized.Split(
                new[] { '\n', ';' },
                StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();

            foreach (var part in rawParts)
            {
                var s = part.TrimEnd();
                if (!string.IsNullOrWhiteSpace(s))
                    result.Add(s);
            }

            return result.ToArray();
        }

        private static string[] SplitLines(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<string>();

            var normalized = text.Replace("\r\n", "\n");

            var lines = normalized.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd('\r', ' ');
            }

            return lines;
        }

        // ================== /admin ==================

        [HttpGet("admin")]
        public IActionResult Index()
        {
            var info = GetCurrentUser();

            if (info == null)
            {
                Context.Response.StatusCode = 401;
                return Json(new { message = "Не авторизован" });
            }

            var (userId, role) = info.Value;

            var isAdmin = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isEmployee = string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isEmployee)
            {
                Context.Response.StatusCode = 403;
                return Json(new { message = "Доступ запрещён" });
            }

            var model = new
            {
                Title = "Административная панель",
                Message = $"Вы успешно вошли как {role} (ID: {userId}).",
                UserRole = role
            };

            return new PageResult("Template/Page/admin-index.thtml", model);
        }

        // ================== /admin/hotels – список отелей ==================

        [HttpGet("admin/hotels")]
        public IActionResult ListHotels()
        {
            try
            {
                RequireAdminOrEmployee();

                var hotels = _hotelRepository.GetAll();

                var sb = new StringBuilder();
                foreach (var h in hotels)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{WebUtility.HtmlEncode(h.Id.ToString())}</td>");
                    sb.Append($"<td>{WebUtility.HtmlEncode(h.Name)}</td>");
                    sb.Append($"<td>{WebUtility.HtmlEncode(h.Slug)}</td>");
                    sb.Append("<td>");
                    sb.Append($"<a href=\"/admin/hotels/details?id={h.Id}\">Редактировать</a>");
                    sb.Append(" | ");
                    sb.Append(
                        $"<a href=\"/admin/hotels/delete?id={h.Id}\" " +
                        "onclick=\"return confirm('Точно удалить этот отель со всеми данными?');\">Удалить</a>"
                    );
                    sb.Append("</td>");
                    sb.Append("</tr>");
                }

                var model = new
                {
                    Title = "Управление отелями",
                    HotelsRows = sb.ToString()
                };

                return new PageResult("Template/Page/admin-hotels-list.thtml", model);
            }
            catch (UnauthorizedAccessException ex)
            {
                Context.Response.StatusCode = 401;
                return Json(new { message = ex.Message });
            }
        }

        [HttpGet("admin/hotels/delete")]
        public IActionResult DeleteHotel()
        {
            try
            {
                RequireAdminOrEmployee();

                var idStr = GetQueryParameter("id");
                if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var hotelId))
                {
                    Context.Response.StatusCode = 400;
                    return Json(new { message = "Некорректный id отеля" });
                }

                var hotel = _hotelRepository.GetById(hotelId);
                if (hotel == null)
                {
                    Context.Response.StatusCode = 404;
                    return Json(new { message = "Отель не найден" });
                }

                var place = _hotelPlaceInfoRepository.GetByHotelId(hotelId);
                if (place != null)
                    _hotelPlaceInfoRepository.Delete(place.Id);

                var desc = _hotelDescriptionRepository.GetByHotelId(hotelId);
                if (desc != null)
                    _hotelDescriptionRepository.Delete(desc.Id);

                var services = _hotelServiceRepository.GetByHotelId(hotelId).ToList();
                foreach (var s in services)
                    _hotelServiceRepository.Delete(s.Id);

                var roomTypes = _roomTypeRepository.GetByHotel(hotelId).ToList();
                foreach (var rt in roomTypes)
                    _roomTypeRepository.Delete(rt.Id);

                _hotelRepository.Delete(hotelId);

                return new RedirectResult("/admin/hotels");
            }
            catch (UnauthorizedAccessException ex)
            {
                Context.Response.StatusCode = 401;
                return Json(new { message = ex.Message });
            }
        }

        // ================== GET /admin/hotels/details ==================

        [HttpGet("admin/hotels/details")]
        public IActionResult EditHotelDetails()
        {
            try
            {
                RequireAdminOrEmployee();

                var idStr = GetQueryParameter("id");
                if (!int.TryParse(idStr, out var hotelId))
                {
                    Context.Response.StatusCode = 400;
                    return Json(new { message = "Некорректный id отеля" });
                }

                var hotel = _hotelRepository.GetById(hotelId);
                if (hotel == null)
                {
                    Context.Response.StatusCode = 404;
                    return Json(new { message = "Отель не найден" });
                }

                var desc = _hotelDescriptionRepository.GetByHotelId(hotelId);
                var place = _hotelPlaceInfoRepository.GetByHotelId(hotelId);
                var services = _hotelServiceRepository.GetByHotelId(hotelId);
                var roomTypes = _roomTypeRepository.GetByHotel(hotelId);

                var contactsText = (hotel.Contacts != null && hotel.Contacts.Length > 0)
                    ? string.Join("\n", hotel.Contacts)
                    : string.Empty;

                var inRoomServicesText = BuildServicesLines(services, "В номере");
                var childServicesText = BuildServicesLines(services, "Для детей");
                var onSiteServicesText = BuildServicesLines(services, "Услуги на территории");
                var entertainmentServicesText = BuildServicesLines(services, "Развлечения и спорт");

                string descYearOpened = string.Empty;
                string descYearRenovated = string.Empty;
                string descTotalArea = string.Empty;
                string descBuildingInfo = string.Empty;

                if (desc != null)
                {
                    descYearOpened = desc.YearOpened.ToString();
                    descYearRenovated = desc.YearRenovated.ToString();
                    descTotalArea = desc.TotalAreaSquareMeters.ToString(CultureInfo.InvariantCulture);
                    descBuildingInfo = desc.BuildingInfo ?? string.Empty;
                }

                string roomTypesText = string.Empty;
                if (roomTypes != null && roomTypes.Any())
                {
                    var sb = new StringBuilder();
                    foreach (var rt in roomTypes.OrderBy(r => r.Id))
                    {
                        sb.Append(rt.Name).Append(';')
                          .Append(rt.View).Append(';')
                          .Append(rt.BedConfiguration).Append(';')
                          .Append(rt.MaxOccupancy).Append(';')
                          .Append(rt.AreaSquareMeters).AppendLine();
                    }

                    roomTypesText = sb.ToString().TrimEnd();
                }

                // превью-фото
                var (photos, coverPhoto) = HotelPhotosHelper.ResolvePhotos(hotel);
                var hotelPhotoPreviewUrl = coverPhoto ?? (hotel.PhotoUrl ?? string.Empty);

                var model = new
                {
                    Title = "Редактирование отеля: " + hotel.Name,

                    HotelId = hotel.Id,
                    HotelName = hotel.Name,
                    HotelSlug = hotel.Slug,
                    HotelCityId = hotel.CityId.ToString(),
                    HotelType = hotel.HotelType,

                    HotelPhotoUrl = hotel.PhotoUrl ?? string.Empty,
                    HotelPhotoPreviewUrl = hotelPhotoPreviewUrl,

                    HotelPrice = hotel.Price.ToString(),
                    HotelDescription = hotel.Description,

                    Place_Address = place?.Address ?? string.Empty,
                    Place_City = place?.City ?? string.Empty,
                    Place_Country = place?.Country ?? string.Empty,
                    Place_DistanceToAirport = place?.DistanceToAirport ?? string.Empty,
                    Place_DistanceToCenter = place?.DistanceToCenter ?? string.Empty,
                    Place_DistanceToBeach = place?.DistanceToBeach ?? string.Empty,

                    Desc_YearOpened = descYearOpened,
                    Desc_YearRenovated = descYearRenovated,
                    Desc_TotalArea = descTotalArea,
                    Desc_BuildingInfo = descBuildingInfo,

                    ContactsText = contactsText,

                    InRoomServicesText = inRoomServicesText,
                    ChildServicesText = childServicesText,
                    OnSiteServicesText = onSiteServicesText,
                    EntertainmentServicesText = entertainmentServicesText,

                    RoomTypesText = roomTypesText
                };

                return new PageResult("Template/Page/admin-hotels-edit.thtml", model);
            }
            catch (UnauthorizedAccessException ex)
            {
                Context.Response.StatusCode = 401;
                return Json(new { message = ex.Message });
            }
        }

        // ================== POST /admin/hotels/details ==================
        [HttpPost("admin/hotels/details")]
        public IActionResult SaveHotelDetails()
        {
            try
            {
                RequireAdminOrEmployee();

                var form = ReadForm();

                // id берём из query (?id=...) или из скрытого поля HotelId
                string idStr = GetQueryParameter("id");
                if (string.IsNullOrWhiteSpace(idStr))
                    form.TryGetValue("HotelId", out idStr);

                if (string.IsNullOrWhiteSpace(idStr) || !int.TryParse(idStr, out var hotelId))
                    return Json(new { message = "Некорректный id отеля" }, 400);

                var hotel = _hotelRepository.GetById(hotelId);
                if (hotel == null)
                    return Json(new { message = "Отель не найден" }, 404);

                // ===== Hotel (основная информация) =====
                if (form.TryGetValue("HotelName", out var hotelName))
                    hotel.Name = hotelName;

                if (form.TryGetValue("HotelSlug", out var hotelSlug))
                    hotel.Slug = hotelSlug;

                if (form.TryGetValue("HotelCityId", out var cityIdStr) &&
                    int.TryParse(cityIdStr, out var cityId))
                    hotel.CityId = cityId;

                if (form.TryGetValue("HotelType", out var hotelType))
                    hotel.HotelType = hotelType;

                if (form.TryGetValue("HotelPhotoUrl", out var photoUrl))
                    hotel.PhotoUrl = photoUrl;

                if (form.TryGetValue("HotelPrice", out var priceStr) &&
                    int.TryParse(priceStr, out var price))
                    hotel.Price = price;

                if (form.TryGetValue("HotelDescription", out var hotelDesc))
                    hotel.Description = hotelDesc;

                if (form.TryGetValue("ContactsText", out var contactsText))
                    hotel.Contacts = SplitLines(contactsText ?? string.Empty);

                // ===== ЗАГРУЗКА ФОТО: ЧИСТЫЙ base64 + имя файла =====
                if (form.TryGetValue("HotelPhotoUploadBase64", out var photoBase64) &&
                    !string.IsNullOrWhiteSpace(photoBase64))
                {
                    // чистим мусор
                    photoBase64 = photoBase64.Trim()
                                             .Replace("\r", "")
                                             .Replace("\n", "");

                    photoBase64 = photoBase64.Replace(' ', '+');

                    Console.WriteLine($"[Admin] Hotel {hotelId}: base64 length = {photoBase64.Length}");

                    // определяем расширение файла
                    string ext = ".jpg";
                    if (form.TryGetValue("HotelPhotoFileName", out var fileNameRaw) &&
                        !string.IsNullOrWhiteSpace(fileNameRaw))
                    {
                        try
                        {
                            var e = Path.GetExtension(fileNameRaw);
                            if (!string.IsNullOrWhiteSpace(e))
                                ext = e;
                        }
                        catch
                        {

                        }
                    }

                    try
                    {
                        var bytes = Convert.FromBase64String(photoBase64);
                        Console.WriteLine($"[Admin] Hotel {hotelId}: decoded bytes = {bytes.Length}");

                        if (bytes.Length > 0)
                        {
                            // физический путь: bin/Debug/net9.0/Public/uploads/hotels/{id}
                            var root = AppContext.BaseDirectory;
                            var uploadDir = Path.Combine(root, "Public", "uploads", "hotels", hotelId.ToString());
                            Directory.CreateDirectory(uploadDir);

                            var fileName = "photo-" + DateTime.UtcNow.Ticks + ext;
                            var fullPath = Path.Combine(uploadDir, fileName);
                            File.WriteAllBytes(fullPath, bytes);

                            Console.WriteLine($"[Admin] Hotel {hotelId}: photo saved to {fullPath}");

                            // в БД кладём ДИРЕКТОРИЮ
                            var relativeDir = "/uploads/hotels/" + hotelId;
                            hotel.PhotoUrl = relativeDir;
                        }
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"[Admin] Hotel {hotelId}: base64 FormatException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Admin] Hotel {hotelId}: error while saving photo: {ex}");
                    }
                }
                // ===== конец блока с фото =====

                _hotelRepository.Update(hotel.Id, hotel);

                // ===== HotelPlaceInfo =====
                var place = _hotelPlaceInfoRepository.GetByHotelId(hotelId)
                           ?? new HotelPlaceInfo { HotelId = hotelId };

                form.TryGetValue("Place_Address", out var placeAddress);
                form.TryGetValue("Place_City", out var placeCity);
                form.TryGetValue("Place_Country", out var placeCountry);
                form.TryGetValue("Place_DistanceToAirport", out var placeAirport);
                form.TryGetValue("Place_DistanceToCenter", out var placeCenter);
                form.TryGetValue("Place_DistanceToBeach", out var placeBeach);

                place.Address = placeAddress ?? string.Empty;
                place.City = placeCity ?? string.Empty;
                place.Country = placeCountry ?? string.Empty;
                place.DistanceToAirport = placeAirport ?? string.Empty;
                place.DistanceToCenter = placeCenter ?? string.Empty;
                place.DistanceToBeach = placeBeach ?? string.Empty;

                if (place.Id == 0)
                    _hotelPlaceInfoRepository.Add(place);
                else
                    _hotelPlaceInfoRepository.Update(place.Id, place);

                // ===== HotelDescription =====
                var desc = _hotelDescriptionRepository.GetByHotelId(hotelId)
                           ?? new HotelDescription { HotelId = hotelId };

                if (form.TryGetValue("Desc_YearOpened", out var yoStr) &&
                    int.TryParse(yoStr, out var yo))
                    desc.YearOpened = yo;

                if (form.TryGetValue("Desc_YearRenovated", out var yrStr) &&
                    int.TryParse(yrStr, out var yr))
                    desc.YearRenovated = yr;

                if (form.TryGetValue("Desc_TotalArea", out var totalAreaStr) &&
                    decimal.TryParse(
                        (totalAreaStr ?? string.Empty).Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var area))
                    desc.TotalAreaSquareMeters = area;

                if (form.TryGetValue("Desc_BuildingInfo", out var bInfo))
                    desc.BuildingInfo = bInfo ?? string.Empty;

                if (desc.Id == 0)
                    _hotelDescriptionRepository.Add(desc);
                else
                    _hotelDescriptionRepository.Update(desc.Id, desc);

                // ===== HotelServices =====
                void ReplaceServices(string category, string fieldName)
                {
                    var current = _hotelServiceRepository.GetByHotelId(hotelId)
                        .Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var svc in current)
                        _hotelServiceRepository.Delete(svc.Id);

                    form.TryGetValue(fieldName, out var text);
                    var items = SplitServiceItems(text ?? string.Empty);

                    foreach (var item in items)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;

                        var name = item.TrimEnd();

                        _hotelServiceRepository.Add(new HotelServiceEntity
                        {
                            HotelId = hotelId,
                            Category = category,
                            Name = name
                        });
                    }
                }

                ReplaceServices("В номере", "InRoomServicesText");
                ReplaceServices("Для детей", "ChildServicesText");
                ReplaceServices("Услуги на территории", "OnSiteServicesText");
                ReplaceServices("Развлечения и спорт", "EntertainmentServicesText");

                // ===== RoomTypes (типы номеров) =====
                if (form.TryGetValue("RoomTypesText", out var roomTypesText))
                {
                    var rtLines = SplitLines(roomTypesText ?? string.Empty)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();

                    if (rtLines.Count > 0)
                    {
                        var existingRoomTypes = _roomTypeRepository.GetByHotel(hotelId)
                            .OrderBy(rt => rt.Id)
                            .ToList();

                        var parsed = new List<(string Name, string View, string Beds, int? MaxOcc, int? AreaSq)>();

                        foreach (var line in rtLines)
                        {
                            var parts = line.Split(';');

                            string name = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                            string view = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                            string beds = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                            int? maxOccNew = null;
                            int? areaSqNew = null;

                            if (parts.Length > 3 && int.TryParse(parts[3].Trim(), out var mo))
                                maxOccNew = mo;

                            if (parts.Length > 4 && int.TryParse(parts[4].Trim(), out var areaSq))
                                areaSqNew = areaSq;

                            parsed.Add((name, view, beds, maxOccNew, areaSqNew));
                        }

                        for (int i = 0; i < parsed.Count; i++)
                        {
                            var p = parsed[i];

                            if (i < existingRoomTypes.Count)
                            {
                                var rt = existingRoomTypes[i];

                                if (!string.IsNullOrWhiteSpace(p.Name))
                                    rt.Name = p.Name;
                                if (!string.IsNullOrWhiteSpace(p.View))
                                    rt.View = p.View;
                                if (!string.IsNullOrWhiteSpace(p.Beds))
                                    rt.BedConfiguration = p.Beds;
                                if (p.MaxOcc.HasValue)
                                    rt.MaxOccupancy = p.MaxOcc.Value;
                                if (p.AreaSq.HasValue)
                                    rt.AreaSquareMeters = p.AreaSq.Value;

                                _roomTypeRepository.Update(rt.Id, rt);
                            }
                            else
                            {
                                var rt = new RoomType
                                {
                                    HotelId = hotelId,
                                    Name = p.Name,
                                    View = p.View,
                                    BedConfiguration = p.Beds,
                                    MaxOccupancy = p.MaxOcc ?? 0,
                                    AreaSquareMeters = p.AreaSq ?? 0
                                };

                                _roomTypeRepository.Add(rt);
                            }
                        }
                    }
                }

                return new RedirectResult($"/admin/hotels/details?id={hotel.Id}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { message = ex.Message }, 401);
            }
        }
        // ================== СОЗДАНИЕ ОТЕЛЯ ==================

        // GET /admin/hotels/create
        [HttpGet("admin/hotels/create")]
        public IActionResult CreateHotel()
        {
            try
            {
                RequireAdminOrEmployee();

                // можно подставить любые дефолтные значения
                var hotel = new Hotel
                {
                    Name = "Новый отель",
                    Slug = "hotel-" + DateTime.UtcNow.Ticks, // чтобы было уникально
                    CityId = 1,      
                    HotelType = "",
                    PhotoUrl = "",
                    Price = 0,
                    Description = ""
                };

                _hotelRepository.Add(hotel);

                return new RedirectResult($"/admin/hotels/details?id={hotel.Id}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Context.Response.StatusCode = 401;
                return Json(new { message = ex.Message });
            }
        }
    }
}
