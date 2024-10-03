using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.WebAPI;

internal static class PluginReports
{
    private const string EncryptionKey = "PyroCommon";

    private static bool _running;
    internal static async Task Run()
    {
        var listener = new TcpListener(IPAddress.Any, 8055);
        try
        {
            listener.Start();
            _running = true;
            while (_running)
            {
                using var client = await listener.AcceptTcpClientAsync();
                var buffer = new byte[1024];
                var bytesRead = await client.GetStream().ReadAsync(buffer);
                var encryptedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var decryptedMessage = DecryptMessage(encryptedData);
                if ( !decryptedMessage.EndsWith("PyroCommon") ) continue;
                if ( !decryptedMessage.Contains('%') ) continue;
                var plug = decryptedMessage.Split("%")[0];
                var err = decryptedMessage.Split("%")[1].Length - 10;
                await Logging.PyroCommonLog(BasicEmbeds.Warning($"__{plug} Auto Report__\r\n```{err}```"));
            }
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        finally { listener.Stop(); _running = false; }
    }

    private static string DecryptMessage(string encryptedText)
    {
        try
        {
            var parts = encryptedText.Split(':');
            var iv = Convert.FromBase64String(parts[0]);
            var encryptedBytes = Convert.FromBase64String(parts[1]);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32)[..32]);
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