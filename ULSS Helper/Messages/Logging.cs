using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    private static readonly DiscordChannel TsBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);
    private static readonly DiscordChannel PubBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.PublicBotLogChannelId);
    
    //Standard Logging
    internal static async Task ErrLog(string e)
    {
        await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{e}```").SendAsync(TsBotLogCh);//TODO: Check msg size
    }
    
    internal static async Task SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(TsBotLogCh);
    }
    
    //Public Logging
    internal static async Task SendPubLog(DiscordEmbedBuilder e)
    {
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(PubBotLogCh);
    }
    internal static async Task ReportPubLog(DiscordEmbedBuilder e)
    {
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(PubBotLogCh);
    }
}