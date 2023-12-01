using Dapper;
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
    public const string SelectIdForRemoval = "SelectIdForRemoval";
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
    public const string SendFeedback = "SendFeedback";

    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs eventArgs)
    {
        List<string> cacheEventIds = new()
        {
            SelectAttachmentForAnalysis,
            SelectIdForRemoval,
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
                ProcessCache cache = Program.Cache.GetProcess(eventArgs.Message.Id);

                if (eventArgs.Id.Equals(SelectAttachmentForAnalysis))
                {
                    string selectedValue = eventArgs.Values.FirstOrDefault();
                    string[] ids = selectedValue!.Split(SharedLogInfo.OptionValueSeparator);
                    ulong messageId = ulong.Parse(ids[0]);
                    ulong targetAttachmentId = ulong.Parse(ids[1]);
                    DiscordMessage message = await eventArgs.Channel.GetMessageAsync(messageId);
                    DiscordAttachment targetAttachment = message.Attachments.FirstOrDefault(attachment => attachment.Id == targetAttachmentId);

                    if (targetAttachment!.FileName.Contains("RagePluginHook"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        RPHProcess rphProcess;
                        if (cache == null || cache.RphProcess == null || cache.RphProcess.log.AnalysisHasExpired())
                        {
                            rphProcess = new RPHProcess();
                            rphProcess.log = RPHAnalyzer.Run(targetAttachment.Url);
                            rphProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(messageId: eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, rphProcess));
                        }
                        else
                            rphProcess = cache.RphProcess;
                        
                        await rphProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ELS"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        ELSProcess elsProcess;
                        if (cache == null || cache.ElsProcess == null || cache.ElsProcess.log.AnalysisHasExpired())
                        {
                            elsProcess = new ELSProcess();
                            elsProcess.log = ELSAnalyzer.Run(targetAttachment.Url);
                            elsProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, elsProcess));
                        }
                        else
                            elsProcess = cache.ElsProcess;

                        await elsProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("asiloader"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        ASIProcess asiProcess;
                        if (cache == null || cache.AsiProcess == null || cache.AsiProcess.log.AnalysisHasExpired())
                        {
                            asiProcess = new ASIProcess();
                            asiProcess.log = ASIAnalyzer.Run(targetAttachment.Url);
                            asiProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, asiProcess));
                        }
                        else 
                            asiProcess = cache.AsiProcess;
                        
                        await asiProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ScriptHookVDotNet"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        SHVDNProcess shvdnProcess;
                        if (cache == null || cache.ShvdnProcess == null || cache.ShvdnProcess.log.AnalysisHasExpired())
                        {
                            shvdnProcess = new SHVDNProcess();
                            shvdnProcess.log = SHVDNAnalyzer.Run(targetAttachment.Url);
                            shvdnProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, shvdnProcess));
                        }
                        else
                            shvdnProcess = cache.ShvdnProcess;
                        
                        await shvdnProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                }
                if (eventArgs.Id.Equals(SelectIdForRemoval))
                {
                    var selectComp = (DiscordSelectComponent) eventArgs.Message.Components.AsList()[0].Components.AsList()[0];
                    var options = new List<DiscordSelectComponentOption>(selectComp.Options);
                    foreach (var option in selectComp.Options.Where(option => option.Value.Equals(eventArgs.Values.FirstOrDefault()))) options.Remove(option);
                    var db = new DiscordInteractionResponseBuilder();
                    
                    if (options.Count > 0)
                        db.AddComponents(new DiscordSelectComponent(
                            customId: ComponentInteraction.SelectIdForRemoval,
                            placeholder: "Remove Error",
                            options: options));
                    db.AddComponents(new DiscordComponent[]
                    {
                        new DiscordButtonComponent(
                            ButtonStyle.Danger,
                            ComponentInteraction.RphDetailedSendToUser,
                            "Send To User", false,
                            new DiscordComponentEmoji("📨"))});
                    var embed = new DiscordEmbedBuilder(eventArgs.Message.Embeds.FirstOrDefault()!);
                    for (int i = embed.Fields.Count - 1; i > 0; i--) 
                        if (embed.Fields[i].Name.Contains(eventArgs.Values.FirstOrDefault()!)) embed.RemoveFieldAt(i);
                    
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, db.AddEmbed(embed));
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
            if (eventArgs.Id == SendFeedback)
            {
                DiscordInteractionResponseBuilder modal = new();
                modal.WithCustomId(SendFeedback);
                modal.WithTitle("Send Feedback");
                modal.AddComponents(
                    new TextInputComponent(
                        label: "Feedback:", 
                        customId: "feedback", 
                        required: true, 
                        style: TextInputStyle.Paragraph
                    )
                );

                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
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