using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Frimework.Core.HttpResponse
{
    /// <summary>
    /// Простой результат-редирект (302 Found).
    /// </summary>
    public class RedirectResult : IActionResult
    {
        private readonly string _url;

        public RedirectResult(string url)
        {
            _url = url;
        }

        public Task ExecuteAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = 302;
            context.Response.RedirectLocation = _url;

            // Можно отдать короткое тело, чтобы браузеры не ругались.
            var bytes = Encoding.UTF8.GetBytes($"<html><body>Redirecting to <a href=\"{_url}\">{_url}</a></body></html>");
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;
            return context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
