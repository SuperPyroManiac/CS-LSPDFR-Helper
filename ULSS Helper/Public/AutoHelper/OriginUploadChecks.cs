using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

internal class OriginUploadChecks
{
    internal static async Task<bool> Check(MessageCreateEventArgs ctx)
    {
        if (Database.LoadUsers().Any(x => x.UID == ctx.Author.Id.ToString() && x.Blocked == 1) && !ctx.Author.IsBot)
        {
            await ctx.Message.RespondAsync(BasicEmbeds.Error(
                $"You are blacklisted from the bot!\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!"));
            await ctx.Message.DeleteAsync();
            return false;
        }

        if (ctx.Message.Attachments.Count != 1 && !ctx.Author.IsBot)
        {
            await ctx.Message.RespondAsync(
                BasicEmbeds.Error("Please only send a single `RagePluginHook.log` file!"));
            await ctx.Message.DeleteAsync();
            return false;
        }

        if (ctx.Message.Attachments.Count == 1 && !ctx.Author.IsBot)
        {
            var attach = ctx.Message.Attachments.FirstOrDefault();

            if (!attach!.FileName.Equals("RagePluginHook.log"))
            {
                await ctx.Message.RespondAsync(BasicEmbeds.Error("This is not a `RagePluginHook.log` file!"));
                Logging.SendPubLog(BasicEmbeds.Warning(
                    $"__Rejected upload!__\r\n"
                    + $">>> Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attach.FileName}\r\n"
                    + $"Size: {attach.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attach.Url})\r\n\r\n"
                    + $"Reason denied: Incorrect name", true
                ));
                await ctx.Message.DeleteAsync();
                return false;
            }

            if (attach!.FileSize > 10000000)
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
                await ctx.Message.DeleteAsync();
                return false;
            }

            AutoCase findCase = null;
            foreach (var autocase in Database.LoadCases()
                         .Where(autocase => autocase.OwnerID.Equals(ctx.Author.Id.ToString()))) findCase = autocase;

            if (findCase != null && findCase.Solved == 0)
            {
                var wng = await ctx.Message.RespondAsync(
                    BasicEmbeds.Error($"You already have an open case!\r\nCheck <#{findCase.ChannelID}>"));
                await ctx.Message.DeleteAsync();
                return false;
            }
            
            await ctx.Message.DeleteAsync();
            return true;
        }

        Thread.Sleep(4000);
        await ctx.Message.DeleteAsync();
        return false;
    }
}