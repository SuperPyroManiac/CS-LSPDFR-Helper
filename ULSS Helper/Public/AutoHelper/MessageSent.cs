using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.AutoHelper.Modules.Process_Files;
using RPHProcess = ULSS_Helper.Public.AutoHelper.Modules.Process_Files.RPHProcess;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static void MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        try
        {
            if (ctx.Channel.IsPrivate) return;
            if (Database.LoadCases().Any(x => x.ChannelID == ctx.Channel.Id.ToString()) && !ctx.Author.IsBot)
            {
                var ac = Database.LoadCases().First(x => x.ChannelID == ctx.Channel.Id.ToString());
                if (ac.OwnerID == ctx.Author.Id.ToString())
                {
                    ac.Timer = ac.TsRequested switch
                    {
                        1 => 24,
                        0 => 6,
                        _ => ac.Timer
                    };
                    Database.EditCase(ac);

                    foreach (var error in Database.LoadErrors().Where(error => error.Level == "AUTO"))
                    {
                        Console.WriteLine("We detected an error auto");
                        var errregex = new Regex(error.Regex);
                        var errmatch = errregex.Match(ctx.Message.Content);
                        if (errmatch.Success)
                            ctx.Message.RespondAsync(BasicEmbeds.Public($"# __ULSS AutoHelper__\r\n{error.Solution}")).GetAwaiter();
                    }
                    
                    if (ctx.Message.Attachments.Count == 0) return;
                    foreach (var attach in ctx.Message.Attachments)//TODO: Blacklist over 6MB file size on match
                    {
                        switch (attach.FileName)
                        {
                            case "RagePluginHook.log":
                                RPHProcess.ProcessLog(attach, ctx);
                                break;
                            case "ELS.log":
                                ELSProcess.ProcessLog(attach, ctx);
                                break;
                            case "asiloader.log":
                                ASIProcess.ProcessLog(attach, ctx);
                                break;
                            case "ScriptHookVDotNet.log":
                                SHVDNProcess.ProcessLog(attach, ctx);
                                break;
                            default: 
                                if (attach.FileName.EndsWith(".png") || attach.FileName.EndsWith(".jpg"))
                                    ImageProcess.ProcessImage(attach, ctx).GetAwaiter();
                                if (attach.FileName.EndsWith(".log"))
                                    ctx.Message.RespondAsync(BasicEmbeds.Public(
                                        "## __ULSS AutoHelper__\r\nThis file is not supported or is not named correctly!"));
                                break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}