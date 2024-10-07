﻿using System;
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
                if (!listener.Pending()) continue;

                using var client = await listener.AcceptTcpClientAsync();
                var stream = client.GetStream();
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer);

                var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (request.StartsWith("GET"))
                {
                    await HandleHttpRequest(request, stream);
                }
                else
                {
                    var encryptedData = request;
                    var decryptedMessage = DecryptMessage(encryptedData);
                    if (!decryptedMessage.EndsWith("PyroCommon")) continue;
                    if (!decryptedMessage.Contains('%')) continue;

                    var delimiterIndex = decryptedMessage.IndexOf('%');
                    if (delimiterIndex == -1) continue;

                    var plug = decryptedMessage.Substring(0, delimiterIndex);
                    var err = decryptedMessage.Substring(delimiterIndex + 1);
                    err = err.Substring(0, err.Length - "PyroCommon".Length).Trim();

                    await Logging.PyroCommonLog(BasicEmbeds.Warning($"__{plug} Auto Report__\r\n```{err}```"));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            _running = false;
            await Run();
        }
    }

    private static async Task HandleHttpRequest(string request, NetworkStream stream)
    {
        if (request.Contains("/ver/"))
        {
            var pluginName = request.Split('/')[2].Split(' ')[0];
            var version = "Not Found!";
            if (Program.Cache.GetPlugin(pluginName) != null) version = Program.Cache.GetPlugin(pluginName).Version;
            var response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{version}";
            var responseData = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseData);
        }
        else
        {
            var response = "HTTP/1.1 404 Not Found\r\n\r\n";
            var responseData = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseData);
        }
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