using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    //Standard Logging
    internal static void ErrLog(string e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"### Error Detected\r\n```{e}```")
            .SendAsync(Program.Client.GetChannelAsync(1173304071084585050).Result);
    }
    
    internal static void SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"**Sent by: <@{msgSender}> in: <#{chLink}>**")
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(1173304071084585050).Result);
    }
    
    //Public Logging
    internal static void SendPubLog(DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(1173304117557477456).Result);
    }
    internal static void ReportPubLog(DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(547311030477520896).Result);
    }
}