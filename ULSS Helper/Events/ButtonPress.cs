using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs eventArgs)
    {
        try
        {
            ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);

            if (eventArgs.Id.Equals("selectAttachmentForAnalysis"))
            {
                string? selectedValue = eventArgs.Values.FirstOrDefault();
                string[]? ids = selectedValue.Split(LogAnalysisProcess.OptionValueSeparator);
                ulong messageId = ulong.Parse(ids[0]);
                ulong targetAttachmentId = ulong.Parse(ids[1]);
                DiscordMessage? message = await eventArgs.Channel.GetMessageAsync(messageId);
                DiscordAttachment? targetAttachment = message.Attachments.FirstOrDefault(attachment => attachment.Id == targetAttachmentId);

                if (targetAttachment.FileName.Contains("RagePluginHook"))
                {
                    await eventArgs.Interaction.DeferAsync(true);
                    RPHProcess rphProcess = new RPHProcess();
                    rphProcess.log = RPHAnalyzer.Run(targetAttachment.Url);
                    rphProcess.log.MsgId = cache.OriginalMessage.Id;
                    Program.Cache.SaveProcess(messageId: eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, rphProcess));
                    await rphProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                    return;
                }
                if (targetAttachment.FileName.Contains("ELS"))
                {
                    await eventArgs.Interaction.DeferAsync(true);
                    ELSProcess elsProcess = new ELSProcess();
                    elsProcess.log = ELSAnalyzer.Run(targetAttachment.Url);
                    elsProcess.log.MsgId = cache.OriginalMessage.Id;
                    Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, elsProcess));
                    await elsProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                    return;
                }
            }

            //RPH log reader buttons
            if (eventArgs.Id is "send" or "send2") await cache.RphProcess.SendMessageToUser(eventArgs);
            if (eventArgs.Id == "info") await cache.RphProcess.SendDetailedInfoMessage(eventArgs);
        
            //ELS log reader buttons
            if (eventArgs.Id is "sendElsToUser" or "sendElsDetailsToUser") await cache.ElsProcess.SendMessageToUser(eventArgs);
            if (eventArgs.Id == "elsDetails") await cache.ElsProcess.SendDetailedInfoMessage(eventArgs);
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}