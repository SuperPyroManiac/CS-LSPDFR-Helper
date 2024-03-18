using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class Logging
{
    //Standard Logging
    internal static async Task ErrLog(string e)
    {
        var tsBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);

        if (e.Length >= 4000)
        {
            var ee = e.Substring(0, 3800);
            await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{ee}...+more```\r\n**Over character limit! See log for details!").SendAsync(tsBotLogCh);
            return;
        }
        await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{e}```").SendAsync(tsBotLogCh);
    }
    
    internal static async Task SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        var tsBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);
        e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(tsBotLogCh);
    }
    
    //Public Logging
    internal static async Task SendPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.PublicBotLogChannelId);
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(pubBotLogCh);
    }
    internal static async Task ReportPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.Env.PublicBotLogChannelId);
        await new DiscordMessageBuilder().WithEmbed(e).SendAsync(pubBotLogCh);
    }
}