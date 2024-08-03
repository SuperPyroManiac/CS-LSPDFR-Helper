using DSharpPlus.EventArgs;

namespace LSPDFRHelper.Functions.Messages;

public static class AutoReplies
{
    public static async Task MonitorMessages(MessageCreatedEventArgs ctx)
    {
        if (Program.Cache.GetCases().All(x => x.ChannelId != ctx.Channel.Id)) return;
        if (ctx.Message.Attachments.Count == 0) return;
        var dltMsg = false;
        
        foreach (var attach in ctx.Message.Attachments)
        {
            if (attach.FileName!.EndsWith(".log") && !attach.FileName.Equals("RagePluginHook.log", StringComparison.CurrentCultureIgnoreCase) && !attach.FileName.Equals("els.log", StringComparison.CurrentCultureIgnoreCase) && !attach.FileName.Equals("asiloader.log", StringComparison.CurrentCultureIgnoreCase) )
                await ctx.Message.RespondAsync(BasicEmbeds.Public("## __LSPDFR AutoHelper__\r\n>>> This log is not correct. Supported logs are:\r\n- RagePluginHook.log\r\n- .xml or .meta files.\r\n\r\nEnsure RagePluginHook logs are from the main directory, and not the logs folder!"));
            
            if (attach.FileName.EndsWith(".rcr"))
                await ctx.Message.RespondAsync(BasicEmbeds.Public("## __LSPDFR AutoHelper__\r\n>>> This file is not supported! Please send `RagePluginHook.log` from your main GTA directory instead. Not the logs folder."));
                
            if (attach.FileName.EndsWith(".exe") || attach.FileName.EndsWith(".dll") || attach.FileName.EndsWith(".asi"))
            {
                await ctx.Message.RespondAsync(BasicEmbeds.Error($"__LSPDFR AutoHelper__\r\n{ctx.Author.Mention}\r\n>>> Do not upload executable files!\r\nFile: {attach.FileName}"));
                dltMsg = true;
            }

            if (attach.FileName.Equals("message.txt"))
                await ctx.Message.RespondAsync(BasicEmbeds.Public("## __LSPDFR AutoHelper__\r\n>>> Please don't copy and paste the log! Please send `RagePluginHook.log` from your main GTA directory instead by dragging it into Discord."));
        }
        if (dltMsg) await ctx.Message.DeleteAsync();
    }
}