using System.Globalization;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace LSPDFRHelper.Functions;

public class RemoteAPI
{
    private readonly HttpListener _listener = new();
    private string _authToken;
    
    public RemoteAPI(string[] prefixes)
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
        //if ( Program.BotSettings.Env.DbName.Contains("DEV", StringComparison.OrdinalIgnoreCase) || Program.BotSettings.Env.DbName.Contains("PYRO", StringComparison.OrdinalIgnoreCase) ) return;
        _listener.Start();
        
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
            
        response.ContentType = "application/json";
            
        if (!IsAuthenticated(request))
        {
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var errorBuffer = Encoding.UTF8.GetBytes("{\"error\":\"Unauthorized access!\"}");
            response.ContentLength64 = errorBuffer.Length;
            await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            response.OutputStream.Close();
            return;
        }

        if ( request.HttpMethod == "GET" && request.Url!.AbsolutePath == "/api/lsPlugs" )
        {
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(DbManager.GetPlugins()));

            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        response.OutputStream.Close();
    }
    
    private static bool IsAuthenticated(HttpListenerRequest request)
    {
        var allowedDrift = TimeSpan.FromMinutes(1);
    
        var authorizationHeader = request.Headers["Authorization"];
        Console.WriteLine(authorizationHeader);
        Console.WriteLine(DateTime.UtcNow);
        if (authorizationHeader == null) return false;
    
        var parts = authorizationHeader.Split(' ');
        if (parts.Length != 2 || parts[0] != "Session") return false;
    
        var token = parts[1];
        DateTime tokenTime;
    
        if (!DateTime.TryParseExact(token, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out tokenTime))
        {
            return false;
        }

        var currentTime = DateTime.UtcNow;
        return currentTime - tokenTime <= allowedDrift;
    }
}