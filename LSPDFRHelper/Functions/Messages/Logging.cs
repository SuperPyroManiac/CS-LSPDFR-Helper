using DSharpPlus.Entities;

namespace LSPDFRHelper.Functions.Messages;

public class Logging
{
    //Standard Logging
    public static async Task ErrLog(string e)
    {
        var tsBotLogCh = await Program.BotSettings.ErrorLogs();

        if (e.Length >= 2000)
        {
            var ee = e[..1850];
            await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{ee}...+more```\r\n__**Over character limit! See log for details!**__").SendAsync(tsBotLogCh);
            return;
        }
        await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{e}```").SendAsync(tsBotLogCh);
    }
    
    public static async Task SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e)
    {
        var tsBotLogCh = await Program.BotSettings.BotLogs();
        e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(tsBotLogCh);
    }
    
    public static async Task SendLog(DiscordEmbedBuilder e)
    {
        var tsBotLogCh = await Program.BotSettings.BotLogs();
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(tsBotLogCh);
    }
    
    //Public Logging
    public static async Task SendPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.BotSettings.BotLogs();
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(pubBotLogCh);
    }
    public static async Task ReportPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.BotSettings.ServerLogs();
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(pubBotLogCh);
    }

    public static async Task PyroCommonLog(DiscordEmbedBuilder e)
    {
        var ch = await Program.Client.GetChannelAsync(1291247289536352257);
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(ch);
    }
}