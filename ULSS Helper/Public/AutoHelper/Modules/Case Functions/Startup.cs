using DSharpPlus;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class Startup
{
    internal static async Task StartAutoHelper()
    {
        await UpdateMsg();
        await CheckUsers.Validate();
        await CheckCases.Validate();
        await CaseMonitor.UpdateMonitor();
    }
    internal static async Task UpdateMsg()
    {
        var cl = Program.Client;
        var ch = await cl.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId);
        var st = Database.AutoHelperStatus();
        List<DiscordMessage> msgPurge = [];
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Public("# __ULSS AutoHelper__");
        await foreach (var msg in ch.GetMessagesAsync(100))
        {
            if (msg.Embeds.Count <= 0) continue;
            if (msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper")) origMsg = msg;
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");

        embed.Description = embed.Description + 
                            "\r\n> *AutoHelper accepts a variety of file types and will attempt to find any issues. " +
                            "Accepted file types are RPH, ASI, ELS, SHVDN logs, XML and META files.*" +
                            "\r\n> This can solve a lot of standard issues, but for more advanced problems, " +
                            "you may wish to use the support channels to ask for human help." +
                            "\r\n\r\n## __Rules Of Use__" +
                            "\r\n> - Do not use the bot for proxy support! This includes uploading logs that are not yours!" +
                            "\r\n> - Do not send modified logs to 'test' the bot. We already have, it wont crash." +
                            "\r\n> - Do not upload logs or files greater than 3MB! Access will instantly be revoked." +
                            "\r\n> - Do not spam cases. You can upload multiple logs to a single case." +
                            "\r\n\r\n## __Other Info__" +
                            "\r\n>>> Anyone can join and assist in cases! use **/JoinCase** to do so! " +
                            "You may request help from a TS in a case using the button, " +
                            "but only do this if you have tried all the steps the bot has given you. " +
                            "If you just instantly request help without trying, your access will be revoked!";
        if (!st) embed.AddField("AutoHelper Disabled!", "System has been disabled by staff temporarily!");

        var dmsg = new DiscordMessageBuilder().AddEmbed(embed);
        dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.OpenCase, "Open Case", !st));
        
        
        await dmsg.ModifyAsync(origMsg);
    }
}
