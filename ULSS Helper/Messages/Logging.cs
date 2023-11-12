using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    internal static void ErrLog(string e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"### Error Detected\r\n```{e}```")
            .SendAsync(Program.Client.GetChannelAsync(1168638324131508316).Result);
    }
    
    internal static void sendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"### Sent by: <@{msgSender}> - In: <#{chLink}>")
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(1168638324131508316).Result);
    }
    //internal static void sendMissing(ulong msgID, )
}