using System.Net;
using System.Text;

namespace LSPDFRHelper.Functions.WebAPI;

internal static class VersionAPI
{
    internal static async Task GetVersion(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        var pluginName = request.RawUrl!.Split('/')[2];
        var version = "Not Found!";
        var plugin = Program.Cache.GetPlugin(pluginName);
        if (plugin != null) version = plugin.Version;

        var responseData = Encoding.UTF8.GetBytes(version);
        response.ContentType = "text/plain";
        response.ContentLength64 = responseData.Length;
        await response.OutputStream.WriteAsync(responseData, 0, responseData.Length);
    }
}