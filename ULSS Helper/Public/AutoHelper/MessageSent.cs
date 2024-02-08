using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;
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

                    if (ctx.Message.Attachments.Count == 0) return;
                    foreach (var attach in ctx.Message.Attachments)
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