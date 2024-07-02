using DSharpPlus;
using DSharpPlus.Entities;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions.Verifications;

public static class AutoHelper
{
    public static async Task UpdateMainAhMessage()
    {
        var ch = await Program.Client.GetChannelAsync(Program.Settings.AutoHelperChId);
        var st = DbManager.AutoHelperStatus();
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Ts("# __ULSS AutoHelper__", null);
        await foreach (var msg in ch.GetMessagesAsync())
        {
            if (msg.Embeds.Count <= 0) continue;
            var first = msg.Embeds.FirstOrDefault();
            if (first!.Description != null && msg.Embeds.FirstOrDefault()!.Description!.Contains("ULSS AutoHelper")) origMsg = msg;
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
        if (!st) embed.Description += "\r\n\r\n## __AutoHelper Disabled!__\r\n>>> **System has been disabled by staff temporarily!**";
        
        await new DiscordMessageBuilder()
            .AddEmbed(embed)
            .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Success, CustomIds.OpenCase, "Open Case", !st))
            .ModifyAsync(origMsg);
    }
    
    public static async Task UpdateAhMonitor()
    {
        var ch = await Program.Client.GetChannelAsync(Program.Settings.MonitorChId);
        List<DiscordMessage> msgPurge = [];
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Ts("# __AutoHelper Active Cases__", null);
        await foreach (var msg in ch.GetMessagesAsync())
        {
            if (msg.Embeds.Count == 0) msgPurge.Add(msg);
            foreach (var emb in msg.Embeds)
            {
                if (!emb.Description!.Contains("AutoHelper Active Cases") &&
                    !emb.Description.Contains("Help Requested!"))
                {
                    msgPurge.Add(msg);
                }
                if (emb.Description.Contains("AutoHelper Active Cases"))
                {
                    origMsg = msg;
                }
            }
        }
        foreach (var msg in msgPurge)
        {
            await ch.DeleteMessageAsync(msg);
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");

        var allCases = Program.Cache.GetCases().Where(ac => !ac.Solved).ToList().OrderBy(ac => ac.ExpireDate);
        foreach (var ac in allCases.TakeWhile(ac => embed.Fields.Count < 16))
        {
            if (embed.Fields.Count == 15)
            {
                embed.AddField("..And More", "There are too many cases to show!");
                break;
            }
            
            if (!ac.Solved)
                embed.AddField($"__<#{ac.ChannelId}>__",
                    $">>> Author: <@{ac.OwnerId}>"
                    + $"\r\nHelp Requested: {Convert.ToBoolean(ac.TsRequested)}"
                    + $"\r\nCreated: {Formatter.Timestamp(ac.CreateDate.ToLocalTime())} | AutoClose: {Formatter.Timestamp(ac.ExpireDate.ToLocalTime())}");
        }
        if (embed.Fields.Count == 0) embed.AddField("None", "No open cases!");

        await new DiscordMessageBuilder().AddEmbed(embed).ModifyAsync(origMsg);
    }
    
    // public static Task<int> ValidateOpenCases()
    // {
    // }
    //
    // public static Task<int> ValidateClosedCases()
    // {
    // }
}