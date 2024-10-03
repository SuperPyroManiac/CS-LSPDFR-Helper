/*using System.Globalization;
using System.Net;
using System.Text;
using LSPDFRHelper.Functions.Processors.RPH;
using Newtonsoft.Json;

namespace LSPDFRHelper.Functions;

public class RemoteApi
{
    private readonly HttpListener _listener = new();

    public RemoteApi(string[] prefixes)
    {
        if (!HttpListener.IsSupported)
        {
            throw new NotSupportedException("HttpListener is not supported.");
        }
        foreach (var prefix in prefixes)
        {
            _listener.Prefixes.Add(prefix);
        }
    }

    public async Task Start()
    {
        //if (Program.BotSettings.Env.DbName.Equals("ULSSHelper")) return;
        if (Program.BotSettings.Env.SchemaName.Contains("DEV", StringComparison.OrdinalIgnoreCase)) return;

        _listener.Start();

        while (true)
        {
            var context = await _listener.GetContextAsync();
            _ = HandleRequestsAsync(context);
        }
    }

    private async Task HandleRequestsAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var statusCode = ValidateRequest(request);

        if (statusCode == HttpStatusCode.OK)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            var requestData = await reader.ReadToEndAsync();
            var rphProcessor = new RphProcessor();
            rphProcessor.Log = await RPHValidater.Run(requestData, true);

            var responseText = rphProcessor.Log.LogModified
                ? "Rejected!"
                : JsonConvert.SerializeObject(rphProcessor.Log);

            SendResponse(response, rphProcessor.Log.LogModified ? HttpStatusCode.Unauthorized : HttpStatusCode.OK, responseText);
        }
        else
        {
            SendResponse(response, statusCode, "Unauthorized!");
        }

        response.Close();
    }

    private HttpStatusCode ValidateRequest(HttpListenerRequest request)
    {
        return request.HttpMethod != "POST" || !IsAuthenticated(request)
            ? HttpStatusCode.Unauthorized
            : HttpStatusCode.OK;
    }

    private void SendResponse(HttpListenerResponse response, HttpStatusCode statusCode, string content)
    {
        var buffer = Encoding.UTF8.GetBytes(content);

        response.StatusCode = (int)statusCode;
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }
    
    private bool IsAuthenticated(HttpListenerRequest request)
    {
        const int timeWindowInMinutes = 3; // Allow a 3-minute window
        var authorizationHeader = request.Headers["Authorization"];
        if (authorizationHeader == null) 
        {
            Console.WriteLine("Authorization header is null.");
            return false;
        }

        var parts = authorizationHeader.Split(' ');
        if (parts.Length != 2 || parts[0] != "Session")
        {
            Console.WriteLine($"Invalid authorization format: {authorizationHeader}");
            return false;
        }

        var token = parts[1];

        if (!DateTime.TryParseExact(token, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var tokenTime)) 
        {
            Console.WriteLine($"Invalid token format: {token}");
            return false;
        }

        var currentTime = DateTime.UtcNow;
        var timeDifference = Math.Abs((currentTime - tokenTime).TotalMinutes);
        return timeDifference <= timeWindowInMinutes;
    }
}*/