using System.Net;
using System.Text;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.WebAPI;

internal static class ErrorReportAPI
{
    internal static async Task SendError(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        
        using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
        var encryptedMessage = await reader.ReadToEndAsync();
        var decryptedMessage = WebApiManager.DecryptMessage(encryptedMessage, "PyroCommon");

        if (!decryptedMessage.EndsWith("PyroCommon")) return;
        if (!decryptedMessage.Contains('%')) return;

        var delimiterIndex = decryptedMessage.IndexOf('%');
        if (delimiterIndex == -1) return;

        var plug = decryptedMessage.Substring(0, delimiterIndex);
        var err = decryptedMessage.Substring(delimiterIndex + 1);
        err = err.Substring(0, err.Length - "PyroCommon".Length).Trim();

        var parts = err.Split([" at "], 2, StringSplitOptions.None);
        var errorMessage = parts[0];
        var stackTrace = parts.Length > 1 ? parts[1] : string.Empty;

        await Logging.PyroCommonLog(BasicEmbeds.Warning($"__{plug} Auto Report__\r\n: ```{errorMessage}```\r\nStack Trace:\r\n```{stackTrace}```"));
        response.StatusCode = 200;
    }
}