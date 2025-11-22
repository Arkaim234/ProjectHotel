using MiniHttpServer.Frimework.Core.Abstracts;
using MiniHttpServer.Frimework.Core.Handlers;
using MiniHttpServer.Frimework.Settings;
using System.Net;
using System.Text;


namespace MiniHttpServer.Frimework.Server
{
    public class HttpServer
    {
        private HttpListener _listener = new();
        private JsonEntity _config;
        private CancellationToken _token;

        public HttpServer(JsonEntity config) { _config = config; }

        public void Start(CancellationToken token)
        {
            _token = token;
            _listener = new HttpListener();
            string url = "http://" + _config.Domain + ":" + _config.Port + "/";
            _listener.Prefixes.Add(url);
            _listener.Start();
            Console.WriteLine("Сервер запущен! Проверяй в браузере: " + url);
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        protected async void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext? context = null;

            try
            {
                if (!_listener.IsListening || _token.IsCancellationRequested)
                    return;

                try
                {
                    context = _listener.EndGetContext(result);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (HttpListenerException ex)
                {
                    Console.WriteLine($"[HttpServer] Listener error: {ex}");
                    if (!_token.IsCancellationRequested && _listener.IsListening)
                        Receive();
                    return;
                }

                var staticFilesHandler = new StaticFilesHandler();
                var endpointsHandler = new EndpointsHandlers();
                staticFilesHandler.Successor = endpointsHandler;

                try
                {
                    await staticFilesHandler.HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HttpServer] Unhandled request error: {ex}");

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
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpServer] Fatal listener error: {ex}");
            }
            finally
            {
                try
                {
                    if (!_token.IsCancellationRequested && _listener.IsListening)
                        Receive();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HttpServer] Error while scheduling next Receive(): {ex}");
                }
            }
        }
    }
}