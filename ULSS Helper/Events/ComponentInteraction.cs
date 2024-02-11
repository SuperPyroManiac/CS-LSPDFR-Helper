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
using ULSS_Helper.Public.AutoHelper;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    // Msc
    public const string SelectAttachmentForAnalysis = "SelectAttachmentForAnalysis";
    public const string SelectIdForRemoval = "SelectIdForRemoval";
    
    // Public
    public const string SendFeedback = "SendFeedback";
    public const string RequestHelp = "RequestHelp";
    public const string MarkSolved = "MarkSolved";
    public const string JoinCase = "JoinCase";
    public const string OpenCase = "OpenCase";

    // RPH log analysis events
    public const string RphGetQuickInfo = "RphGetQuickInfo";
    public const string RphGetDetailedInfo = "RphGetDetailedInfo";
    public const string RphGetAdvancedInfo = "RphGetAdvancedInfo";
    public const string RphQuickSendToUser = "RphQuickInfoSendToUser";
    public const string RphDetailedSendToUser = "RphDetailedSendToUser";

    // ELS log analysis events
    public const string ElsGetQuickInfo = "ElsGetQuickInfo";
    public const string ElsGetDetailedInfo = "ElsGetDetailedInfo";
    public const string ElsQuickSendToUser = "ElsQuickInfoSendToUser";
    public const string ElsDetailedSendToUser = "ElsDetailedSendToUser";

    // ASI log analysis events
    public const string AsiGetQuickInfo = "AsiGetQuickInfo";
    public const string AsiGetDetailedInfo = "AsiGetDetailedInfo";
    public const string AsiQuickSendToUser = "AsiQuickInfoSendToUser";
    public const string AsiDetailedSendToUser = "AsiDetailedSendToUser";

    // SHVDN log analysis events
    public const string ShvdnGetQuickInfo = "ShvdnGetQuickInfo";
    public const string ShvdnGetDetailedInfo = "ShvdnGetDetailedInfo";
    public const string ShvdnQuickSendToUser = "ShvdnQuickInfoSendToUser";
    public const string ShvdnDetailedSendToUser = "ShvdnDetailedSendToUser";

    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs eventArgs)
    {
        List<string> cacheEventIds =
        [
            SelectAttachmentForAnalysis,
            SelectIdForRemoval,
            RphGetQuickInfo,
            RphGetDetailedInfo,
            RphGetAdvancedInfo,
            RphQuickSendToUser,
            RphDetailedSendToUser,
            ElsGetQuickInfo,
            ElsGetDetailedInfo,
            ElsQuickSendToUser,
            ElsDetailedSendToUser,
            AsiGetQuickInfo,
            AsiGetDetailedInfo,
            AsiQuickSendToUser,
            AsiDetailedSendToUser,
            ShvdnGetQuickInfo,
            ShvdnGetDetailedInfo,
            ShvdnQuickSendToUser,
            ShvdnDetailedSendToUser
        ];
        try
        {
            if (cacheEventIds.Any(eventId => eventId == eventArgs.Id))
            {
                var cache = Program.Cache.GetProcess(eventArgs.Message.Id);

                if (eventArgs.Id.Equals(SelectAttachmentForAnalysis))
                {
                    var selectedValue = eventArgs.Values.FirstOrDefault();
                    var ids = selectedValue!.Split(SharedLogInfo.OptionValueSeparator);
                    var messageId = ulong.Parse(ids[0]);
                    var targetAttachmentId = ulong.Parse(ids[1]);
                    var message = await eventArgs.Channel.GetMessageAsync(messageId);
                    var targetAttachment = message.Attachments.FirstOrDefault(attachment => attachment.Id == targetAttachmentId);
                    
                    if (targetAttachment!.FileName.Contains("RagePluginHook"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        RPHProcess rphProcess;
                        if (ProcessCache.IsCacheUsagePossible("RagePluginHook", cache, message.Attachments.ToList()))
                            rphProcess = cache.RphProcess;
                        else
                        {
                            rphProcess = new RPHProcess();
                            rphProcess.log = RPHAnalyzer.Run(targetAttachment.Url).Result;
                            rphProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(messageId: eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, rphProcess));
                        }

                        await rphProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ELS"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        ELSProcess elsProcess;
                        if (ProcessCache.IsCacheUsagePossible("ELS", cache, message.Attachments.ToList()))
                            elsProcess = cache.ElsProcess;
                        else
                        {
                            elsProcess = new ELSProcess();
                            elsProcess.log = ELSAnalyzer.Run(targetAttachment.Url).Result;
                            elsProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, elsProcess));
                        }

                        await elsProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("asiloader"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        ASIProcess asiProcess;
                        if (ProcessCache.IsCacheUsagePossible("asiloader", cache, message.Attachments.ToList()))
                            asiProcess = cache.AsiProcess;
                        else 
                        {
                            asiProcess = new ASIProcess();
                            asiProcess.log = ASIAnalyzer.Run(targetAttachment.Url).Result;
                            asiProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, asiProcess));
                        }
                        
                        await asiProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ScriptHookVDotNet"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        SHVDNProcess shvdnProcess;
                        if (ProcessCache.IsCacheUsagePossible("ScriptHookVDotNet", cache, message.Attachments.ToList()))
                            shvdnProcess = cache.ShvdnProcess;
                        else
                        {
                            shvdnProcess = new SHVDNProcess();
                            shvdnProcess.log = SHVDNAnalyzer.Run(targetAttachment.Url).Result;
                            shvdnProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new(eventArgs.Interaction, cache.OriginalMessage, shvdnProcess));
                        }
                        
                        await shvdnProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                }
                if (eventArgs.Id.Equals(SelectIdForRemoval))
                {
	                var selectComp = (DiscordSelectComponent) eventArgs.Message.Components
		                .FirstOrDefault(compRow => compRow.Components.Any(comp => comp.CustomId == SelectIdForRemoval))
		                ?.Components!.FirstOrDefault(comp => comp.CustomId == SelectIdForRemoval);
                    var allComponentsExceptSelect = eventArgs.Message.Components
	                    .FirstOrDefault(compRow => compRow.Components.All(comp => comp.CustomId != SelectIdForRemoval))?.Components;
                    
                    var options = new List<DiscordSelectComponentOption>(selectComp!.Options);
                    var optionsToRemove = selectComp.Options.Where(option => option.Value.Equals(eventArgs.Values.FirstOrDefault()));
                    foreach (var option in optionsToRemove)
                        options.Remove(option);

                    var db = new DiscordInteractionResponseBuilder();
                    if (options.Count > 0)
                        db.AddComponents(
                            new DiscordSelectComponent(
                                customId: ComponentInteraction.SelectIdForRemoval,
                                placeholder: "Remove Error",
                                options: options
                            )
                        );
                    var compRow = new List<DiscordComponent>();
                    foreach (var comp in allComponentsExceptSelect!)
                        compRow.Add(comp);
                    db.AddComponents(compRow);
                    
                    var embed = new DiscordEmbedBuilder(eventArgs.Message.Embeds.FirstOrDefault()!);
                    for (var i = embed.Fields.Count - 1; i > 0; i--) 
                        if (embed.Fields[i].Name.Contains(eventArgs.Values.FirstOrDefault()!)) embed.RemoveFieldAt(i);
                    
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, db.AddEmbed(embed));
                }
                    
                //===//===//===////===//===//===////===//RPH Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is RphGetQuickInfo) 
                    await cache.RphProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                
                if (eventArgs.Id == RphGetDetailedInfo) 
                    await cache.RphProcess.SendDetailedInfoMessage(eventArgs);
                
                if (eventArgs.Id == RphGetAdvancedInfo) 
                    await cache.RphProcess.SendAdvancedInfoMessage(eventArgs);
                
                if (eventArgs.Id is RphQuickSendToUser or RphDetailedSendToUser) 
                    await cache.RphProcess.SendMessageToUser(eventArgs);
            
                //===//===//===////===//===//===////===//ELS Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is ElsGetQuickInfo) 
                    await cache.ElsProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                
                if (eventArgs.Id == ElsGetDetailedInfo) 
                    await cache.ElsProcess.SendDetailedInfoMessage(eventArgs);
                
                if (eventArgs.Id is ElsQuickSendToUser or ElsDetailedSendToUser)
                    await cache.ElsProcess.SendMessageToUser(eventArgs);
                
                //===//===//===////===//===//===////===//ASI Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is AsiGetQuickInfo) 
                    await cache.AsiProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);

                if (eventArgs.Id == AsiGetDetailedInfo) 
                    await cache.AsiProcess.SendDetailedInfoMessage(eventArgs);

                if (eventArgs.Id is AsiQuickSendToUser or AsiDetailedSendToUser)
                    await cache.AsiProcess.SendMessageToUser(eventArgs);
                
                //===//===//===////===//===//===////===//SHVDN Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is ShvdnGetQuickInfo) 
                    await cache.ShvdnProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                
                if (eventArgs.Id == ShvdnGetDetailedInfo) 
                    await cache.ShvdnProcess.SendDetailedInfoMessage(eventArgs);

                if (eventArgs.Id is ShvdnQuickSendToUser or ShvdnDetailedSendToUser)
                    await cache.ShvdnProcess.SendMessageToUser(eventArgs);
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
            
            //===//===//===////===//===//===////===//Request Help Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == RequestHelp)
            {
                DiscordInteractionResponseBuilder modal = new();
                modal.WithCustomId(ModalSubmit.RequestHelp);
                modal.WithTitle($"Requesting help!");
                modal.AddComponents(new TextInputComponent(
                    label: "Explain your issue in detail:",
                    customId: "issueDsc",
                    required: true,
                    style: TextInputStyle.Paragraph
                ));
        
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
            }
            
            //===//===//===////===//===//===////===//Mark Solved Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == MarkSolved)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Database.LoadCases().First(x => x.ChannelID.Equals(eventArgs.Channel.Id.ToString()));

                if (eventArgs.User.Id.ToString().Equals(ac.OwnerID) || eventArgs.Guild.GetMemberAsync(eventArgs.User.Id)
                        .Result.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
                {
                    msg.AddEmbed(BasicEmbeds.Info("Closing case!"));
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
                    await CloseCase.Close(ac);
                    return;
                }
                msg.AddEmbed(BasicEmbeds.Error("You do not own this case!"));
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
            }
            
            //===//===//===////===//===//===////===//Join Case Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == JoinCase)
            {
                var ac = Database.LoadCases().First(x => x.CaseID.Equals(
                    eventArgs.Message.Embeds.First().Description.Split("Case: ")[1].Split("_").First()));
                await Public.AutoHelper.Modules.Case_Functions.JoinCase.Join(ac, eventArgs.User.Id.ToString());
            }
            
            //===//===//===////===//===//===////===//Open Case Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == OpenCase)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                if (Database.LoadUsers().Where(x => x.UID == eventArgs.User.Id.ToString()).FirstOrDefault()!.Blocked == 1)
                {
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                        msg.AddEmbed(BasicEmbeds.Error($"You are blacklisted from the bot!\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!")));
                    return;
                }
        
                AutoCase findCase = null;
                foreach (var autocase in Database.LoadCases()
                             .Where(autocase => autocase.OwnerID.Equals(eventArgs.User.Id.ToString()))) findCase = autocase;

                if (findCase != null && findCase.Solved == 0)
                {
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
                        msg.AddEmbed(BasicEmbeds.Error($"You already have an open case!\r\nCheck <#{findCase.ChannelID}>")));
                    return;
                }

                msg.AddEmbed(BasicEmbeds.Success($"Created new case! {Public.AutoHelper.Modules.Case_Functions.OpenCase.CreateCase(eventArgs).Result.Mention}"));
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
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
