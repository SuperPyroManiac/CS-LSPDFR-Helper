using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Public.AutoHelper;

namespace ULSS_Helper.Public.Events;

public class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        if (Program.Settings.Env.AutoHelperChannelIds.All(x => ctx.Channel == ctx.Guild.GetChannel(x)))
        {
            if (ctx.Message.Attachments.Count != 1 && !ctx.Author.IsBot)
            {
                var wng = await ctx.Message.RespondAsync(BasicEmbeds.Error("Please only send a single `RagePluginHook.log` file!"));
                Thread.Sleep(4000);
                await ctx.Message.DeleteAsync();
                await ctx.Channel.DeleteMessageAsync(wng);
                return;
            }
            if (ctx.Message.Attachments.Count == 1 && !ctx.Author.IsBot)
            {
                var attach = ctx.Message.Attachments.FirstOrDefault();
                
                if (!attach!.FileName.Equals("RagePluginHook.log"))
                {
                    var wng = await ctx.Message.RespondAsync(BasicEmbeds.Error("This is not a `RagePluginHook.log` file!"));
                    Logging.SendPubLog(BasicEmbeds.Warning(
                        $"__Rejected upload!__\r\n"
                        + $">>> Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                        + $"Channel: <#{ctx.Channel.Id}>\r\n"
                        + $"File name: {attach.FileName}\r\n"
                        + $"Size: {attach.FileSize/1000}KB\r\n"
                        + $"[Download Here]({attach.Url})\r\n\r\n"
                        + $"Reason denied: Incorrect name", true
                    ));
                    Thread.Sleep(4000);
                    await ctx.Message.DeleteAsync();
                    await ctx.Channel.DeleteMessageAsync(wng);
                    return;
                }
                
                if (attach!.FileSize > 10000000)
                {
                    var wng = await ctx.Message.RespondAsync(BasicEmbeds.Error(
                        "File is way too big!\r\nYou may not upload anything else until staff review this!"));
                    await ctx.Guild.GetMemberAsync(ctx.Author.Id).Result.
                        GrantRoleAsync(ctx.Guild.GetRole(Program.Settings.Env.BotBlacklistRoleId));
                    Logging.ReportPubLog(BasicEmbeds.Error(
                        $"__Possible bot abuse!__\r\n"
                        + $">>> User has been blacklisted from bot use! (Dunce role added!)\r\n"
                        + $"Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                        + $"Channel: <#{ctx.Channel.Id}>\r\n"
                        + $"File name: {attach.FileName}\r\n"
                        + $"Size: {attach.FileSize / 1000}KB\r\n"
                        + $"[Download Here]({attach.Url})\r\n\r\n"
                        + $"Reason denied: File way too large! (Larger than 10 MB)", true
                    ));
                    Logging.SendPubLog(BasicEmbeds.Error(
                        $"__Rejected upload!__\r\n"
                        + $">>> Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                        + $"Channel: <#{ctx.Channel.Id}>\r\n"
                        + $"File name: {attach.FileName}\r\n"
                        + $"Size: {attach.FileSize / 1000}KB\r\n"
                        + $"[Download Here]({attach.Url})\r\n\r\n"
                        + $"Reason denied: File way too large! (Larger than 10 MB)", true
                    ));
                    Thread.Sleep(4000);
                    await ctx.Message.DeleteAsync();
                    await ctx.Channel.DeleteMessageAsync(wng);
                    return;
                }
            }

            if (!ctx.Author.IsBot)
            {
                var attach = ctx.Message.Attachments.FirstOrDefault();
                var caseId = new Random().Next(int.MaxValue).ToString("x");
                var supportthread = await ctx.Channel.
                    CreateThreadAsync($"AutoHelper - Case: {caseId}", 
                        AutoArchiveDuration.ThreeDays, ChannelType.PublicThread);
                await supportthread.SendMessageAsync($"### {ctx.Author.Mention} with case: {caseId}");
                await supportthread.SendMessageAsync(
                    BasicEmbeds.Public("## Your log has been uploaded!\r\n" +
                                       "Depending on the size, this may take a moment to process!"));
                
                await AutoRPH.ProccessLog(RPHAnalyzer.Run(attach!.Url), ctx, supportthread);
            }
            if (ctx.Message.MessageType == MessageType.ThreadCreated) await ctx.Message.DeleteAsync();
        }
    }
}