using DSharpPlus.Entities;

namespace LSPDFR_Helper.Functions.Messages;

internal class Logging//TODO: redo this all
{
    //Standard Logging
    internal static async Task ErrLog(string e)
    {
        var tsBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.BotLogChId);

        if (e.Length >= 2000)
        {
            var ee = e[..1850];
            await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{ee}...+more```\r\n__**Over character limit! See log for details!**__").SendAsync(tsBotLogCh);
            return;
        }
        await new DiscordMessageBuilder().WithContent($"### Error Detected\r\n```{e}```").SendAsync(tsBotLogCh);
    }
    
    internal static async Task SendLog(ulong chLink, ulong msgSender, DiscordEmbedBuilder e, bool blame = true)
    {
        var tsBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.BotLogChId);
        if (blame) e.AddField("Sent By", $"<@{msgSender}> in: <#{chLink}>");
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(tsBotLogCh);
    }
    
    //Public Logging
    internal static async Task SendPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.BotLogChId);
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(pubBotLogCh);
    }
    internal static async Task ReportPubLog(DiscordEmbedBuilder e)
    {
        var pubBotLogCh = await Program.Client.GetChannelAsync(Program.Settings.ReportChId);
        await new DiscordMessageBuilder().AddEmbed(e).SendAsync(pubBotLogCh);
    }
}