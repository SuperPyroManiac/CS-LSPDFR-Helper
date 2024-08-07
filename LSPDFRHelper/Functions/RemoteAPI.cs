using System.Globalization;
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
        //if ( Program.BotSettings.Env.DbName.Contains("DEV", StringComparison.OrdinalIgnoreCase) ) return;
        Console.WriteLine("Starting...");
        try
        {
            _listener.Start();
        }
        catch ( Exception e )
        {
            Console.WriteLine(e);
            throw;
        }
        Console.WriteLine("Started!");

        
        while ( true )
        {
            var ctx = await _listener.GetContextAsync();
            await HandleRequestsAsync(ctx);

        }
    }

    private async Task HandleRequestsAsync(HttpListenerContext ctx)
    {
        var request = ctx.Request;
        var response = ctx.Response;


        if (request.HttpMethod == "POST" && request.Url!.AbsolutePath == "/api/lsRph" && IsAuthenticated(request))
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            byte[] buffer;
            var requestData = await reader.ReadToEndAsync();
            var rphProcessor = new RphProcessor();
            rphProcessor.Log = await RPHValidater.Run(requestData, true);
            if ( rphProcessor.Log.LogModified )
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                buffer = Encoding.UTF8.GetBytes("Rejected!");
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
                
            var result = JsonConvert.SerializeObject(rphProcessor.Log);

            buffer = Encoding.UTF8.GetBytes(result);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            byte[] buffer = Encoding.UTF8.GetBytes("Unauthorized!");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        

        response.Close();
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
        DateTime tokenTime;

        if (!DateTime.TryParseExact(token, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tokenTime)) 
        {
            Console.WriteLine($"Invalid token format: {token}");
            return false;
        }

        var currentTime = DateTime.UtcNow;
        var timeDifference = Math.Abs((currentTime - tokenTime).TotalMinutes);
        //Console.WriteLine($"Token time: {tokenTime}, Current time: {currentTime}, Difference: {timeDifference} minutes");

        return timeDifference <= timeWindowInMinutes;
    }

}