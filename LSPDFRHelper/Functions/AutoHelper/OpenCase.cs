using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.AutoHelper;

public class OpenCase
{
    public static async Task<DiscordThreadChannel> CreateCase(ulong user, ulong guild)
    {
        var caseId = new Random().Next(int.MaxValue).ToString("x");

        var ahCh = await Program.Client.GetChannelAsync(Program.Cache.GetServer(guild).AutoHelperChId);
        var ch = await ahCh.CreateThreadAsync($"AutoHelper - Case: {caseId}", DiscordAutoArchiveDuration.ThreeDays, DiscordChannelType.PrivateThread);
        var caseMsg = new DiscordMessageBuilder();
        caseMsg.AddEmbed(BasicEmbeds.Ts(
            $"# __AutoHelper Case: {caseId}__" +
            $"\r\n> **You have opened a new case! You can upload the following files to be automatically checked:**" +
            $"\r\n> - RagePluginHook.log" +
            $"\r\n> - - __**Not**__ from your `logs` folder!" +
            $"\r\n> - ELS.log" +
            //$"\r\n> - asiloader.log" +
            //$"\r\n> - ScriptHookVDotNet.log" +
            $"\r\n> - .xml and .meta files" +
            //$"\r\n> - Screenshots of .png or .jpg - BETA" +
            $"\r\n\r\n*Do not abuse the request help button. Only use it if you have tried all the steps provided and have exhausted your own options.*" +
            $"\r\n\r\n__This bot is maintained by https://dsc.PyrosFun.com!__", null));
        caseMsg.AddComponents(
            new DiscordButtonComponent(DiscordButtonStyle.Success, CustomIds.MarkSolved, "Mark Solved", false,
                new DiscordComponentEmoji("👍")),
            new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RequestHelp, "Request Help", false,
                new DiscordComponentEmoji("❓")));
        var mem = await Program.Client.Guilds[guild].GetMemberAsync(user);
        await ch.AddThreadMemberAsync(mem);

        var newCase = new AutoCase
        {
            CaseId = caseId,
            OwnerId = user,
            ChannelId = ch.Id,
            ServerId = ch.Guild.Id,
            Solved = false,
            TsRequested = false,
            CreateDate = DateTime.Now.ToUniversalTime(),
            ExpireDate = DateTime.Now.ToUniversalTime().AddHours(6)
        };
        DbManager.AddCase(newCase);

        var smsg = await ch.SendMessageAsync(caseMsg);
        await smsg.PinAsync();
        return ch;
    }
}