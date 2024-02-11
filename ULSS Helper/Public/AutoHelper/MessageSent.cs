using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Public.Modules.Process_Logs;
using RPHProcess = ULSS_Helper.Public.Modules.Process_Logs.RPHProcess;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static void MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        try
        {
            if (ctx.Channel.IsPrivate) return;
            if (ctx.Message.MessageType == MessageType.ThreadCreated) ctx.Channel.DeleteMessageAsync(ctx.Message).GetAwaiter();
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
                        var errregex = new Regex(error.Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        var errmatch = errregex.Match(ctx.Message.Content);
                        if (errmatch.Success)
                            ctx.Message.RespondAsync(BasicEmbeds.Public($"## __ULSS AutoHelper__\r\n`Response ID is: {error.ID}`\r\n{error.Solution}")).GetAwaiter();
                    }
                    
                    if (ctx.Message.Attachments.Count == 0) return;
                    foreach (var attach in ctx.Message.Attachments)
                    {
                        switch (attach.FileName)
                        {
                            case "RagePluginHook.log":
                                RPHProcess.ProcessLog(attach, ctx).GetAwaiter();
                                break;
                            case "ELS.log":
                                ELSProcess.ProcessLog(attach, ctx).GetAwaiter();
                                break;
                            case "asiloader.log":
                                ASIProcess.ProcessLog(attach, ctx).GetAwaiter();
                                break;
                            case "ScriptHookVDotNet.log":
                                SHVDNProcess.ProcessLog(attach, ctx).GetAwaiter();
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