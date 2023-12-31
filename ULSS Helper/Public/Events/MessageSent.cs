using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;

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
                if (!ctx.Message.Attachments.FirstOrDefault()!.FileName.Equals("RagePluginHook.log"))
                {
                    var attachment = ctx.Message.Attachments.FirstOrDefault()!;
                    var wng = await ctx.Message.RespondAsync(BasicEmbeds.Error("This is not a `RagePluginHook.log` file!"));
                    Logging.SendPubLog(BasicEmbeds.Warning(
                        $"__Rejected upload!__\r\n"
                        + $">>> Sender: <@{ctx.Author.Id}> ({ctx.Author.Username})\r\n"
                        + $"Channel: <#{ctx.Channel.Id}>\r\n"
                        + $"File name: {attachment.FileName}\r\n"
                        + $"Size: {attachment.FileSize/1000}KB\r\n"
                        + $"[Download Here]({attachment.Url})\r\n\r\n"
                        + $"Reason denied: Incorrect name", true
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
                var supportthread = await ctx.Channel.CreateThreadAsync($"Test Support Thread {new Random().Next(0, 1000)}", AutoArchiveDuration.ThreeDays,
                    ChannelType.PublicThread);
                await supportthread.SendMessageAsync($"{ctx.Author.Mention}\r\nHere is your ticket!");
                Thread.Sleep(10000);
                await supportthread.DeleteAsync();
                await ctx.Message.DeleteAsync();
            }
            if (ctx.Message.MessageType == MessageType.ThreadCreated) await ctx.Message.DeleteAsync();
        }
    }
}