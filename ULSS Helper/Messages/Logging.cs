using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    //Standard Logging
    internal static void ErrLog(string e)
    {
        new DiscordMessageBuilder()
            .WithContent($"### Error Detected\r\n```{e}```")
            .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId).Result);
    }
    
    internal static void SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId).Result);
    }
    
    //Public Logging
    internal static void SendPubLog(DiscordEmbedBuilder e)
    {
        new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.PublicBotLogChannelId).Result);
    }
    internal static void ReportPubLog(DiscordEmbedBuilder e)
    {
        new DiscordMessageBuilder()
            .WithEmbed(e)
            .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.PublicBotReportsChannelId).Result);
    }
}