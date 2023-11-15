using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    public const string SelectAttachmentForAnalysis = "SelectAttachmentForAnalysis";
    public const string RphGetDetailedInfo = "RphGetDetailedInfo";
    public const string RphQuickSendToUser = "RphQuickInfoSendToUser";
    public const string RphDetailedSendToUser = "RphDetailedSendToUser";
    public const string ElsGetDetailedInfo = "ElsGetDetailedInfo";
    public const string ElsQuickSendToUser = "ElsQuickInfoSendToUser";
    public const string ElsDetailedSendToUser = "ElsDetailedSendToUser";
    public const string AsiGetDetailedInfo = "AsiGetDetailedInfo";
    public const string AsiQuickSendToUser = "AsiQuickInfoSendToUser";
    public const string AsiDetailedSendToUser = "AsiDetailedSendToUser";
    public const string ShvdnGetDetailedInfo = "ShvdnGetDetailedInfo";
    public const string ShvdnQuickSendToUser = "ShvdnQuickInfoSendToUser";
    public const string ShvdnDetailedSendToUser = "ShvdnDetailedSendToUser";

    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs eventArgs)
    {
        List<string> cacheEventIds = new()
        {
            SelectAttachmentForAnalysis,
            RphGetDetailedInfo,
            RphQuickSendToUser,
            RphDetailedSendToUser,
            ElsGetDetailedInfo,
            ElsQuickSendToUser,
            ElsDetailedSendToUser,
            AsiGetDetailedInfo,
            AsiQuickSendToUser,
            AsiDetailedSendToUser,
            ShvdnGetDetailedInfo,
            ShvdnQuickSendToUser,
            ShvdnDetailedSendToUser
        };
        try
        {
            if (cacheEventIds.Any(eventId => eventId == eventArgs.Id))
            {
                ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);

                if (eventArgs.Id.Equals(SelectAttachmentForAnalysis))
                {
                    string? selectedValue = eventArgs.Values.FirstOrDefault();
                    string[]? ids = selectedValue.Split(SharedLogInfo.OptionValueSeparator);
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
                    if (targetAttachment.FileName.Contains("asiloader"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        ASIProcess asiProcess = new ASIProcess();
                        asiProcess.log = ASIAnalyzer.Run(targetAttachment.Url);
                        asiProcess.log.MsgId = cache.OriginalMessage.Id;
                        Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, asiProcess));
                        await asiProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ScriptHookVDotNet"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        SHVDNProcess shvdnProcess = new SHVDNProcess();
                        shvdnProcess.log = SHVDNAnalyzer.Run(targetAttachment.Url);
                        shvdnProcess.log.MsgId = cache.OriginalMessage.Id;
                        Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, shvdnProcess));
                        await shvdnProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                }
                    
                //===//===//===////===//===//===////===//RPH Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is RphQuickSendToUser or RphDetailedSendToUser) 
                    await cache.RphProcess.SendMessageToUser(eventArgs);
                
                if (eventArgs.Id == RphGetDetailedInfo) 
                    await cache.RphProcess.SendDetailedInfoMessage(eventArgs);
            
                //===//===//===////===//===//===////===//ELS Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is ElsQuickSendToUser or ElsDetailedSendToUser)
                    await cache.ElsProcess.SendMessageToUser(eventArgs);
                
                if (eventArgs.Id == ElsGetDetailedInfo) 
                    await cache.ElsProcess.SendDetailedInfoMessage(eventArgs);
                
                //===//===//===////===//===//===////===//ASI Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is AsiQuickSendToUser or AsiDetailedSendToUser)
                    await cache.AsiProcess.SendMessageToUser(eventArgs);
                
                if (eventArgs.Id == AsiGetDetailedInfo) 
                    await cache.AsiProcess.SendDetailedInfoMessage(eventArgs);
                
                //===//===//===////===//===//===////===//SHVDN Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is ShvdnQuickSendToUser or ShvdnDetailedSendToUser)
                    await cache.ShvdnProcess.SendMessageToUser(eventArgs);
                
                if (eventArgs.Id == ShvdnGetDetailedInfo) 
                    await cache.ShvdnProcess.SendDetailedInfoMessage(eventArgs);
            }
            
            //===//===//===////===//===//===////===//Send Feedback Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == "SendFeedback")
            {
                DiscordInteractionResponseBuilder modal = new();
                modal.WithTitle($"Send Feedback").WithCustomId("SendFeedback").AddComponents(
                    new TextInputComponent("Feedback:", "feedback", required: true, style: TextInputStyle.Paragraph));
                Console.Write("We got here");
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
                Console.Write("and its fucked");
            }
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}