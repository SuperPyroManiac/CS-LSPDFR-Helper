using System.Net;
using System.Security.Cryptography;
using System.Text;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.WebAPI;

internal static class WebApiManager
{
    private static bool _running;

    internal static async Task Run()
    {
        Console.WriteLine("Starting web api server...");
        var listener = new HttpListener();
        listener.Prefixes.Add("http://*:8055/");
            
        try
        {
            listener.Start();
            _running = true;

            while (_running)
            {
                try
                {
                    var context = await listener.GetContextAsync(); // Handle HTTP requests
                    _ = Task.Run(async () => await HandleRequest(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error handling client: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            _running = false;
            Console.WriteLine("Restarting server after failure...");
            await Task.Delay(5000);
        }
    }

    private static async Task HandleRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;
            
            if (request.HttpMethod == "GET" && request.RawUrl != null && request.RawUrl.Contains("/ver/")) await VersionAPI.GetVersion(context);
            if ( request.HttpMethod == "POST" && request.RawUrl != null && request.RawUrl.Contains("/report") ) await ErrorReportAPI.SendError(context);
            else
            {
                response.StatusCode = 404;
                var responseData = "Not Found"u8.ToArray();
                await response.OutputStream.WriteAsync(responseData);
            }
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
    }


    internal static string DecryptMessage(string encryptedText, string encryptionKey)
    {
        try
        {
            var parts = encryptedText.Split(':');
            var iv = Convert.FromBase64String(parts[0]);
            var encryptedBytes = Convert.FromBase64String(parts[1]);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32)[..32]);
            aes.IV = iv;

            using var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);
            var decryptedBytes = decrypt.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            return $"Error decrypting message: {ex.Message}";
        }
    }
}