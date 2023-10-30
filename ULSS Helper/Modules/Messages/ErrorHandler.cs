using DSharpPlus;
using DSharpPlus.Entities;

namespace ULSS_Helper.Modules.Messages;

internal class ErrorHandler
{
    internal static void ErrLog(string e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"# Error Detected\r\n```{e}```")
            .SendAsync(Program.Client.GetChannelAsync(1168438186939273276).Result);
    }
}