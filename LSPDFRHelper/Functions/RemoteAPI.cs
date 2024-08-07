using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml.Serialization;
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
        if ( Program.BotSettings.Env.DbName.Contains("DEV", StringComparison.OrdinalIgnoreCase) || Program.BotSettings.Env.DbName.Contains("PYRO", StringComparison.OrdinalIgnoreCase) ) return;
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
            byte[] errorBuffer = Encoding.UTF8.GetBytes("{\"error\":\"Unauthorized access!\"}");
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
    
    private bool IsAuthenticated(HttpListenerRequest request)
    {
        _authToken = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        if (request.Headers["Authorization"] == null) return false;
        return request.Headers["Authorization"] == $"Session {_authToken}";
    }
}