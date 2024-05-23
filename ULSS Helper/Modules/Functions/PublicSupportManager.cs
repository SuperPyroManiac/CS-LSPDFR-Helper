using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Modules.Functions;

public class PublicSupportManager
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        if (Program.Settings.Env.SupportChannelId != ctx.Channel.Id) return;
        
        if (ctx.Message.Attachments.Count == 0) return;
        var dltMsg = false;
        foreach (var attach in ctx.Message.Attachments)
        {
            
            if (!attach.FileName.Equals("RagePluginHook.log"))
                if (attach.FileName.Contains("RagePluginHook") && attach.FileName.EndsWith(".log"))
                    await ctx.Message.RespondAsync(BasicEmbeds.Public("## __ULSS AutoHelper__\r\n\r\nThis log is not correct! Please send your `RagePluginHook.log` from your main GTA directory instead. Not the logs folder."));
                
            if (attach.FileName.EndsWith(".rcr"))
                await ctx.Message.RespondAsync(BasicEmbeds.Public("## __ULSS AutoHelper__\r\nThis file is not supported! Please send your `RagePluginHook.log` from your main GTA directory instead. Not the logs folder."));
                
            if (attach.FileName.EndsWith(".exe") || attach.FileName.EndsWith(".dll") || attach.FileName.EndsWith(".asi"))
            {
                await ctx.Message.RespondAsync(BasicEmbeds.Error(
                    $"__ULSS AutoHelper__\r\n{ctx.Author.Mention}\r\n>>> Do not upload executable files!\r\nFile: {attach.FileName}",
                    true));
                dltMsg = true;
            }
        }
        if (dltMsg) await ctx.Message.DeleteAsync();
    }
}