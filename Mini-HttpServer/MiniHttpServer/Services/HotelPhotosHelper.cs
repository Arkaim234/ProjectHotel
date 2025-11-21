using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniHttpServer.Model;

namespace MiniHttpServer.Services
{
    public static class HotelPhotosHelper
    {
        // Какие расширения считаем фотками
        private static readonly string[] ImageExts = { ".jpg", ".jpeg", ".png", ".webp" };

        /// <summary>
        /// Возвращает:
        /// - photos: все URL фоток
        /// - cover: URL обложки (первая фотка)
        /// </summary>
        public static (List<string> photos, string? cover) ResolvePhotos(Hotel hotel)
        {
            var photos = new List<string>();

            if (hotel == null || string.IsNullOrWhiteSpace(hotel.PhotoUrl))
                return (photos, null);

            var value = hotel.PhotoUrl.Trim();

            // 1) Если это путь к файлу (одна фотка)
            if (IsImageFile(value))
            {
                var url = NormalizeUrl(value);
                photos.Add(url);
                return (photos, url);
            }

            // 2) Иначе считаем, что это ДИРЕКТОРИЯ относительно папки Public
            //    Пример: PhotoUrl = "images/hotels/15"
            var dirRelPath = value.Trim('/', '\\');

            // Физический путь на диске: Public/images/hotels/15
            var dirPhysicalPath = Path.Combine("Public", Path.Combine(dirRelPath.Split('/', '\\')));

            if (!Directory.Exists(dirPhysicalPath))
            {
                // Если папки нет — создаём, но пока ничего не возвращаем
                Directory.CreateDirectory(dirPhysicalPath);
                return (photos, null);
            }

            var files = Directory
                .EnumerateFiles(dirPhysicalPath)
                .Where(f => ImageExts.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => f)
                .ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                // URL для браузера: "/images/hotels/15/1.jpg"
                var url = "/" + Path
                    .Combine(dirRelPath, fileName)
                    .Replace("\\", "/");

                photos.Add(url);
            }

            var cover = photos.FirstOrDefault();
            return (photos, cover);
        }

        private static bool IsImageFile(string path)
        {
            var ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return false;

            return ImageExts.Contains(ext.ToLower());
        }

        private static string NormalizeUrl(string value)
        {
            var normalized = value.Replace('\\', '/').Trim();

            if (!normalized.StartsWith("/"))
                normalized = "/" + normalized.TrimStart('/');

            return normalized;
        }
    }
}
