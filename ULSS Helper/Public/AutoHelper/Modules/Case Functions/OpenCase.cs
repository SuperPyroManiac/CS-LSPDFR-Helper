using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

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
            $"\r\n> - .xml and .meta files" +
            //$"\r\n> - Screenshots of .png or .jpg - BETA" +
            $"\r\n\r\nThis is not to be used as general use tickets! It may be closed at any time if TS deem it so. If you need help with something else, it may be faster to ask in the public support channels!" +
            $"\r\n\r\n__Please check the FAQ for common issues!__"));
        caseMsg.AddComponents([
            new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.MarkSolved, "Mark Solved", false,
                new DiscordComponentEmoji("👍")),
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RequestHelp, "Request Help", true,
                new DiscordComponentEmoji("❓")),
            new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                new DiscordComponentEmoji("📨"))]);
        var tmpuser = await ctx.Guild.GetMemberAsync(ctx.User.Id);
        await supportthread.AddThreadMemberAsync(tmpuser);
        
        var newCase = new AutoCase()
        {
            CaseID = caseId,
            OwnerID = ctx.User.Id.ToString(),
            ChannelID = supportthread.Id.ToString(),
            Solved = 0,
            Timer = 6,
            TsRequested = 0
        };
        Database.AddCase(newCase);

        var smsg = await supportthread.SendMessageAsync(caseMsg);
        await smsg.PinAsync();
        return supportthread;
    }
}