using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        try
        {
            if (Program.Settings.Env.AutoHelperChannelIds.Any(x => ctx.Channel == ctx.Guild.GetChannel(x)))
            {
                if (!OriginUploadChecks.Check(ctx).Result) return;

                var attach = ctx.Message.Attachments.FirstOrDefault();
                var caseId = new Random().Next(int.MaxValue).ToString("x");
                var supportthread = await ctx.Channel.CreateThreadAsync($"AutoHelper - Case: {caseId}",
                    AutoArchiveDuration.ThreeDays, ChannelType.PublicThread);
                await supportthread.SendMessageAsync($"### {ctx.Author.Mention} with case: {caseId}");
                var oldmsg = await supportthread.SendMessageAsync(
                    BasicEmbeds.Public("## Your log has been uploaded!\r\n" +
                                       "Depending on the size, this may take a moment to process!"));
                var newCase = new AutoCase()
                {
                    CaseID = caseId,
                    OwnerID = ctx.Author.Id.ToString(),
                    ChannelID = supportthread.Id.ToString(),
                    ParentID = supportthread.Parent.Id.ToString(),
                    Solved = 0,
                    Timer = 24,
                    TsRequested = 0
                };
                Database.AddCase(newCase);

                var log = RPHAnalyzer.Run(attach!.Url).Result;
                var msg = await AutoRPH.ProccessLog(log, ctx, supportthread);
                //msg.AddFile(new FileStream(Path.Combine(log.FilePath), FileMode.Open, FileAccess.Read), AddFileOptions.CloseStream);
                msg.AddComponents([
                    new DiscordButtonComponent(ButtonStyle.Success, "MarkSolved", "Mark Solved", false,
                        new DiscordComponentEmoji("👍")),
                    new DiscordButtonComponent(ButtonStyle.Danger, "RequestHelp", "Request Help", true,
                        new DiscordComponentEmoji("❓")),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "SendFeedback", "Send Feedback", false,
                        new DiscordComponentEmoji("📨"))]);
                await msg.ModifyAsync(oldmsg);
                await supportthread.SendMessageAsync(
                    BasicEmbeds.Info("This is a BETA - features are missing and subject to change!\r\n" +
                                     "You are not to share this preview outside of ULSS!"));
            }
        }
        catch (Exception e)
        {
            await ctx.Message.RespondAsync(BasicEmbeds.Error("There was an error here!\r\nYou may not upload anything else until staff review this!"));
            var attachment = ctx.Message.Attachments.FirstOrDefault();
            var user = Database.LoadUsers().FirstOrDefault(x => x.UID == ctx.Author.Id.ToString());
            if (user != null) user.Blocked = 1;
            Database.EditUser(user);
            Logging.ReportPubLog(BasicEmbeds.Error(
                $"__Possible bot abuse!__\r\n"
                + $">>> User has been blacklisted from bot use!\r\n"
                + $"Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                + $"Channel: <#{ctx.Channel.Id}>\r\n"
                + $"File name: {attachment.FileName}\r\n"
                + $"Size: {attachment.FileSize / 1000}KB\r\n"
                + $"[Download Here]({attachment.Url})\r\n\r\n"
                + $"Reason denied: Log caused an error! See <#{Program.Settings.Env.TsBotLogChannelId}>", true
            ));
            Logging.SendPubLog(BasicEmbeds.Error(
                $"__Rejected upload!__\r\n"
                + $">>> Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                + $"Channel: <#{ctx.Channel.Id}>\r\n"
                + $"File name: {attachment.FileName}\r\n"
                + $"Size: {attachment.FileSize / 1000}KB\r\n"
                + $"[Download Here]({attachment.Url})\r\n\r\n"
                + $"Reason denied: Log caused an error! See <#{Program.Settings.Env.TsBotLogChannelId}>", true
            ));
            Logging.ErrLog($"Public Log Error: {e}");
            Console.WriteLine(e);
            throw;
        }
    }
}