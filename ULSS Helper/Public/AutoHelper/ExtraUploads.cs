using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Public.AutoHelper;

internal class ExtraUploads
{
    internal static async Task CheckLog(MessageCreateEventArgs ctx)
    {
        var ac = Database.LoadCases().First(x => x.ChannelID == ctx.Channel.Id.ToString());

        if (ac.OwnerID == ctx.Author.Id.ToString())
        {
            ac.Timer = 24;
            Database.EditCase(ac);
        }
        
        if (ctx.Message.Attachments.Count == 1)
        {
            var attach = ctx.Message.Attachments.FirstOrDefault();

            if (attach!.FileSize > 10000000 && attach!.FileName.Equals("RagePluginHook.log"))
            {
                var wng = await ctx.Message.RespondAsync(BasicEmbeds.Error(
                    "File is way too big!\r\nYou may not upload anything else until staff review this!"));
                var user = Database.LoadUsers().FirstOrDefault(x => x.UID == ctx.Author.Id.ToString());
                if (user != null) user.Blocked = 1;
                Database.EditUser(user);
                Logging.ReportPubLog(BasicEmbeds.Error(
                    $"__Possible bot abuse!__\r\n"
                    + $">>> User has been blacklisted from bot use!\r\n"
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
                return;
            }
            
            if (attach!.FileName.Equals("RagePluginHook.log") && ac.OwnerID.Equals(ctx.Author.Id.ToString()))
            {
                var log = RPHAnalyzer.Run(attach!.Url).Result;
                var msg = await AutoRPH.ProccessLog(log, ctx, (DiscordThreadChannel)ctx.Channel);
                //msg.AddFile(new FileStream(Path.Combine(log.FilePath), FileMode.Open, FileAccess.Read), AddFileOptions.CloseStream);
                msg.AddComponents([
                    new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                        new DiscordComponentEmoji("📨"))]);
                await ctx.Message.RespondAsync(msg);
            }
        }
    }
}