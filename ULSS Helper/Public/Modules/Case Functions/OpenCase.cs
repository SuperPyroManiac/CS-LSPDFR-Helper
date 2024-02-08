using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.Modules.Case_Functions;

public class OpenCase
{
    internal static async Task<DiscordThreadChannel> CreateCase(ComponentInteractionCreateEventArgs ctx)
    {
        var caseId = new Random().Next(int.MaxValue).ToString("x");
        var supportthread = await ctx.Channel.CreateThreadAsync($"AutoHelper - Case: {caseId}",
            AutoArchiveDuration.ThreeDays, ChannelType.PublicThread);
        var caseMsg = new DiscordMessageBuilder();
        caseMsg.AddEmbed(BasicEmbeds.Public(
            $"# __AutoHelper Case: {caseId}__" +
            $"\r\n> **You have opened a new case! You can upload the following files to be automatically checked:**" +
            $"\r\n> - RagePluginHook.log" +
            $"\r\n> - ELS.log" +
            $"\r\n> - asiloader.log" +
            $"\r\n> - ScriptHookVDotNet.log" +
            $"\r\n\r\n__Please check the FAQ for common issues!__"));
        caseMsg.AddComponents([
            new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.MarkSolved, "Mark Solved", false,
                new DiscordComponentEmoji("👍")),
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RequestHelp, "Request Help", false,
                new DiscordComponentEmoji("❓")),
            new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                new DiscordComponentEmoji("📨"))]);
        await supportthread.AddThreadMemberAsync(ctx.Guild.GetMemberAsync(ctx.User.Id).Result);
        
        var newCase = new AutoCase()
        {
            CaseID = caseId,
            OwnerID = ctx.User.Id.ToString(),
            ChannelID = supportthread.Id.ToString(),
            ParentID = supportthread.Parent.Id.ToString(),
            Solved = 0,
            Timer = 6,
            TsRequested = 0
        };
        Database.AddCase(newCase);

        await supportthread.SendMessageAsync(caseMsg).Result.PinAsync();
        await UpdateMsg();
        return supportthread;
    }

    internal static async Task UpdateMsg()
    {
        var cl = Program.Client;
        var ch = cl.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId).Result;
        List<DiscordMessage> msgPurge = [];
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Public("# __ULSS AutoHelper__");
        await foreach (var msg in ch.GetMessagesAsync(100))
        {
            if (msg.Embeds.Count == 0)
            {
                msgPurge.Add(msg);
                continue;
            }
            if (!msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper")) msgPurge.Add(msg);
            if (msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper"))
            {
                origMsg = msg;
            }
        }
        foreach (var msg in msgPurge)
        {
            if (msg != null) await ch.DeleteMessageAsync(msg);
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");
        embed.AddField("Early Access",
            "AutoHelper is still a work in progress! It is not perfect and can never fully replace people!");
        embed.AddField("Do not abuse the bot!",
            "This is broad, sending altered logs, other files, etc. Your access will be revoked!");
        embed.AddField("No proxy support!",
            "Do not use information from this bot to help others. Instead redirect them here themselves.");
        embed.AddField("Do not upload other peoples logs!",
            "This is considered proxy support, your access will be revoked!");

        var dmsg = new DiscordMessageBuilder().AddEmbed(embed);
        dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.OpenCase, "Open Case", false));
        
        
        await dmsg.ModifyAsync(origMsg);
    }
}