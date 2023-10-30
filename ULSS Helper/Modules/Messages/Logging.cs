using DSharpPlus.Entities;

namespace ULSS_Helper.Modules.Messages;

internal class Logging
{
    internal static void sendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"Sent by: <@{msgSender}> - In: <#{chLink}>")
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(1168638324131508316).Result);
    }
}