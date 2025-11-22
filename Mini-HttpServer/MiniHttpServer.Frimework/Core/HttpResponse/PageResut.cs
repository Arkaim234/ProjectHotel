using MiniHttpServer.Frimework.Core.HttpResponse;
using System.Net;
using System.Reflection;
using System.Text;
using MiniTemplateEngine;

namespace MiniHttpServer.Frimework.Core.HttpResponse
{
    public class PageResult : IActionResult
    {
        private readonly string _pathTemplate;
        private readonly object? _data;
        public int _statusCode;


        public PageResult(string pathTemplate, object? data, int statusCode = 200)
        {
            _pathTemplate = pathTemplate;
            _data = data;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpListenerContext context)
        {
            try
            {
                IHtmlTemplateRenderer renderer = new HtmlTemplateRenderer();

                string html;

                try
                {
                    html = renderer.RenderFromFile(_pathTemplate, _data ?? new { });
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"[PageResult] Template not found: {_pathTemplate}. {ex.Message}");
                    context.Response.StatusCode = 404;
                    html = "<h1>404 - Page template not found</h1>";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PageResult] Template error: {ex}");
                    context.Response.StatusCode = 500;
                    html = "<h1>500 - Template error</h1>";
                }

                var bytes = Encoding.UTF8.GetBytes(html);

                if (context.Response.OutputStream.CanWrite)
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    await context.Response.OutputStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PageResult] Fatal error: {ex}");
            }
        }

    }
}
