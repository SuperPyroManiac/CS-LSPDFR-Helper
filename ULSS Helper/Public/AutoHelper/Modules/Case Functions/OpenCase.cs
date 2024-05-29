using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
// ReSharper disable IdentifierTypo

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
            $"\r\n> - - __**Not**__ from your `logs` folder!" +
            $"\r\n> - ELS.log" +
            $"\r\n> - asiloader.log" +
            $"\r\n> - ScriptHookVDotNet.log" +
            $"\r\n> - .xml and .meta files" +
            //$"\r\n> - Screenshots of .png or .jpg - BETA" +
            $"\r\n\r\n*Do not abuse the request help button. Only use it if you have tried all the steps provided and have exhausted your own options. Abuse of this feature may result in your access being revoked!*" +
            $"\r\n\r\n__Please check the FAQ for common issues!__"));
        caseMsg.AddComponents([
            new DiscordButtonComponent(ButtonStyle.Success, ComponentInteraction.MarkSolved, "Mark Solved", false,
                new DiscordComponentEmoji("👍")),
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RequestHelp, "Request Help", false,
                new DiscordComponentEmoji("❓")),
            new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                new DiscordComponentEmoji("📨"))]);
        var tmpuser = await Program.GetMember(ctx.User.Id.ToString());
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
        await Database.AddCase(newCase);

        var smsg = await supportthread.SendMessageAsync(caseMsg);
        await smsg.PinAsync();
        await CheckCases.Validate();
        return supportthread;
    }
}