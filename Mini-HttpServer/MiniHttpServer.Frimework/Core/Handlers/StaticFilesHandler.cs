using MiniHttpServer.Frimework.Core.Abstracts;
using MiniHttpServer.Frimework.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Frimework.Core.Handlers
{
    using System.IO;
    using System.Text;

    class StaticFilesHandler : Handler
    {
        public async override Task HandleRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var isGetMethod = request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
                var isStaticFile = request.Url.AbsolutePath.Split('/').Any(x => x.Contains("."));

                if (isGetMethod && isStaticFile)
                {
                    var response = context.Response;

                    byte[]? buffer = null;

                    string path = request.Url.AbsolutePath.Trim('/');

                    buffer = GetResponseBytes.Invoke(path);

                    response.ContentType = MiniHttpServer.Frimework.Shared.ContentType.GetContentType(path.Trim('/'));

                    if (buffer == null)
                    {
                        response.StatusCode = 404;
                        string errorText = "<html><body>404 - Not Found</body></html>";
                        buffer = Encoding.UTF8.GetBytes(errorText);
                    }

                    response.ContentLength64 = buffer.Length;

                    using Stream output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    await output.FlushAsync();

                    Console.WriteLine($"[Static] {request.Url.AbsolutePath} - Status: {response.StatusCode}");
                }
                else if (Successor != null)
                {
                    await Successor.HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticFilesHandler] Error: {ex}");

                try
                {
                    if (context?.Response?.OutputStream?.CanWrite == true)
                    {
                        context.Response.StatusCode = 500;
                        var bytes = Encoding.UTF8.GetBytes("Internal server error");
                        await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                        await context.Response.OutputStream.FlushAsync();
                    }
                }
                catch { }
            }
        }
    }

}
