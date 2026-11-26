using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    internal class Program
    {
        private const string Url = "http://localhost:8080/";
        private const string HtmlFilePath = "index.html";

        static async Task Main()
        {
            EnsureHtmlFileExists();

            using HttpListener listener = new HttpListener();
            listener.Prefixes.Add(Url);
            listener.Start();

            Console.WriteLine($"HTTP-сервер запущено на {Url}");
            Console.WriteLine($"Файл сторінки: {Path.GetFullPath(HtmlFilePath)}");
            Console.WriteLine("Натисніть Ctrl+C для зупинки...\n");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context));
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {request.Url}");

            try
            {
                if (!File.Exists(HtmlFilePath))
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    byte[] errorBytes = Encoding.UTF8.GetBytes("<h1>404 - Файл index.html не знайдено</h1>");
                    response.ContentType = "text/html; charset=utf-8";
                    response.ContentLength64 = errorBytes.Length;
                    await response.OutputStream.WriteAsync(errorBytes);
                    return;
                }

                string htmlContent = await File.ReadAllTextAsync(HtmlFilePath, Encoding.UTF8);
                byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;

                await response.OutputStream.WriteAsync(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка обробки запиту: {ex.Message}");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        private static void EnsureHtmlFileExists()
        {
            if (!File.Exists(HtmlFilePath))
            {
                string defaultHtml = """
                    <!DOCTYPE html>
                    <html lang="uk">
                    <head>
                        <meta charset="UTF-8">
                        <title>Локальний HTTP-сервер</title>
                    </head>
                    <body>
                        <h1>Привіт від локального сервера!</h1>
                        <p>Цю сторінку завантажено з файлу <strong>index.html</strong>.</p>
                    </body>
                    </html>
                    """;
                File.WriteAllText(HtmlFilePath, defaultHtml, Encoding.UTF8);
            }
        }
    }
}
