using DSharpPlus;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class Startup
{
    internal static async Task StartAutoHelper()
    {
        CheckUsers.Validate();
        CheckCases.Validate();
        await UpdateMsg();
        await CaseMonitor.UpdateMonitor();
    }
    internal static async Task UpdateMsg()
    {
        var cl = Program.Client;
        var ch = await cl.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId);
        var st = Database.AutoHelperStatus();
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Public("# __ULSS AutoHelper__");
        await foreach (var msg in ch.GetMessagesAsync(100))
        {
            if (msg.Embeds.Count <= 0) continue;
            if (msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper")) origMsg = msg;
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");

        embed.Description = embed.Description + 
                            "\r\n> The AutoHelper can read a variety of file types and will attempt to find issues. Currently supported log files are RagePluginHook logs, ASI logs, ELS logs and ScriptHookVDotNet logs. The AutoHelper can also read XML and .meta files."+
                            "\r\n> Please note that frequent issues can often be detected, but human assistance may be required for more advanced problems. you may wish to use the support channels to ask for human help." +
                            "\r\n\r\n## __AutoHelper Terms Of Use__" +
                            "\r\n> - Do not use the bot for proxy support. This includes uploading logs that are not yours." +
                            "\r\n> - Do not send modified logs to 'test' the bot. We already have, it won't crash." +
                            "\r\n> - Do not upload logs or files greater than **__3MB__**! Access will instantly be revoked." +
                            "\r\n> - Do not spam cases. You can upload multiple logs to a single case." +
                            "\r\n\r\n## __Other Info__" +
                            "\r\n> Anyone can join and assist in cases, using /JoinCase to do so. You can request help from support staff using the button, but only do so if you have tried all the steps that the bot has given you. If you request help without following the bot advice first, your access to AutoHelper may be revoked!" + 
                            "\r\n\r\n> __Created by: SuperPyroManiac, Hendrik, Hammer__";
        if (!st) embed.AddField("AutoHelper Disabled!", "System has been disabled by staff temporarily!");

        var dmsg = new DiscordMessageBuilder().AddEmbed(embed);
        dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.OpenCase, "Open Case", !st));
        
        
        await dmsg.ModifyAsync(origMsg);
    }
}
