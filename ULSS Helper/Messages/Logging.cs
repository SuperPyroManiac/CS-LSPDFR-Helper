using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    const ulong BotLogChannelId = 1173909572440825866; // 1173304071084585050
    const ulong PublicLogChannelId = 1173304117557477456;
    const ulong PublicLogReportsChannelId = 547311030477520896;

    //Standard Logging
    internal static void ErrLog(string e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent($"### Error Detected\r\n```{e}```")
            .SendAsync(Program.Client.GetChannelAsync(BotLogChannelId).Result);
    }
    
    internal static void SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        var log = new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(BotLogChannelId).Result);
    }
    
    //Public Logging
    internal static void SendPubLog(DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(PublicLogChannelId).Result);
    }
    internal static void ReportPubLog(DiscordEmbedBuilder e)
    {
        var log = new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(PublicLogReportsChannelId).Result);
    }
}