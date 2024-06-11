using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    // Msc
    public const string SelectAttachmentForAnalysis = "SelectAttachmentForAnalysis";
    public const string SelectIdForRemoval = "SelectIdForRemoval";
    
    // Editor
    public const string SelectPluginValueToEdit = "SelectPluginValueToEdit";
    public const string SelectPluginValueToFinish = "SelectPluginValueToFinish";
    public const string SelectErrorValueToEdit = "SelectErrorValueToEdit";
    public const string SelectErrorValueToFinish = "SelectErrorValueToFinish";
    
    // Public
    public const string SendFeedback = "SendFeedback";
    public const string RequestHelp = "RequestHelp";
    public const string MarkSolved = "MarkSolved";
    public const string JoinCase = "JoinCase";
    public const string IgnoreRequest = "IgnoreRequest";
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

    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreatedEventArgs eventArgs)
    {
        List<string> cacheEventIds =
        [
            SelectAttachmentForAnalysis,
            SelectIdForRemoval,
            SelectPluginValueToEdit,
            SelectPluginValueToFinish,
            SelectErrorValueToEdit,
            SelectErrorValueToFinish,
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
                    
                    if (targetAttachment!.FileName!.Contains("RagePluginHook"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        RPHProcess rphProcess;
                        if (ProcessCache.IsCacheUsagePossible("RagePluginHook", cache, message.Attachments.ToList()))
                            rphProcess = cache.RphProcess;
                        else
                        {
                            rphProcess = new RPHProcess();
                            rphProcess.log = await RPHAnalyzer.Run(targetAttachment.Url);
                            rphProcess.log.MsgId = cache.OriginalMessage.Id;
                            ProxyCheck.Run(rphProcess.log, Program.Cache.GetUser(eventArgs.Message.Author!.Id.ToString()), eventArgs.Message);
                            Program.Cache.SaveProcess(messageId: eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, rphProcess));
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
                            elsProcess.log = await ELSAnalyzer.Run(targetAttachment.Url);
                            elsProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, elsProcess));
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
                            asiProcess.log = await ASIAnalyzer.Run(targetAttachment.Url);
                            asiProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, asiProcess));
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
                            shvdnProcess.log = await SHVDNAnalyzer.Run(targetAttachment.Url);
                            shvdnProcess.log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, shvdnProcess));
                        }
                        
                        await shvdnProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                }
                
                //===//===//===////===//===//===////===//Remove errors from bot response before sending it to the public user//===////===//===//===////===//===//===//
                if (eventArgs.Id.Equals(SelectIdForRemoval))
                {
                    // Get the current SelectComponent from the Message that the user interacted with
	                var selectComp = (DiscordSelectComponent) eventArgs.Message.Components
		                .FirstOrDefault(compRow => compRow.Components.Any(comp => comp.CustomId == SelectIdForRemoval))
		                ?.Components!.FirstOrDefault(comp => comp.CustomId == SelectIdForRemoval);
                    // Get a list of all components (like buttons) except for the SelectComponent so we can rebuild the list of components later after modifying the SelectComponent
                    var allComponentsExceptSelect = eventArgs.Message.Components
	                    .FirstOrDefault(compRow => compRow.Components.All(comp => comp.CustomId != SelectIdForRemoval))?.Components;
                    
                    var options = new List<DiscordSelectComponentOption>(selectComp!.Options);
                    var optionsToRemove = selectComp.Options.Where(option => int.Parse(option.Value) == int.Parse(eventArgs.Values.FirstOrDefault()!));
                    // Remove the selected option(s) from the list of options in the SelectComponent. This doesn't affect the list of troubleshooting steps in the embed yet.
                    foreach (var option in optionsToRemove)
                        options.Remove(option);

                    var db = new DiscordInteractionResponseBuilder();
                    // If there are any options left after removing the selected error, rebuild the SelectComponent and add it to the response.
                    if (options.Count > 0)
                        db.AddComponents(
                            new DiscordSelectComponent(
                                customId: SelectIdForRemoval,
                                placeholder: "Remove Error",
                                options: options
                            )
                        );
                    var compRow = new List<DiscordComponent>();
                    foreach (var comp in allComponentsExceptSelect!)
                        compRow.Add(comp);
                    db.AddComponents(compRow);
                    
                    var embed = new DiscordEmbedBuilder(eventArgs.Message.Embeds.FirstOrDefault()!);
                    // Remove the error field with the selected id
                    for (var i = embed.Fields.Count - 1; i > 0; i--)
                    {
                        var fieldName = embed.Fields[i].Name;
                        // If the field name contains "ID:" followed by a number, extract the number to compare it with the selected id (for removal) in eventArgs.Values
                        if (new Regex(@"ID:\s?\d+\D+").IsMatch(fieldName)) 
                        {
                            var idString = fieldName.Split("ID:")[1].Trim(); // ["__```SEVERE ", " 123``` Troubleshooting Steps:__"]
                            idString = Regex.Split(idString, @"\D")[0]; // ["123", "``` Troubleshooting Steps:__"]
                            if (int.Parse(idString) == int.Parse(eventArgs.Values.FirstOrDefault()!))
                                embed.RemoveFieldAt(i);
                        }
                    }
                    
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, db.AddEmbed(embed));
                }
                
                //===//===//===////===//===//===////===//Editor Dropdowns//===////===//===//===////===//===//===//
                if (eventArgs.Id is SelectPluginValueToEdit or SelectPluginValueToFinish)
                {
                    var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, SelectPluginValueToEdit);
                    if (usercache == null)
                    {
                        var bd = new DiscordInteractionResponseBuilder();
                        bd.IsEphemeral = true;
                        bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor!", true));
                        await eventArgs.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                        return;
                    }

                    if (eventArgs.Id == SelectPluginValueToFinish)
                    {
                        await FindPluginMessages.SendDbOperationConfirmation(usercache.Plugin, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, Database.GetPlugin(usercache.Plugin.Name));
                        Database.EditPlugin(usercache.Plugin);
                        Program.Cache.RemoveUserAction(eventArgs.User.Id, SelectPluginValueToEdit);
                        await eventArgs.Message.DeleteAsync();
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType
                            .DeferredMessageUpdate);
                        return;
                    }
                    
                    var value = string.Empty;
                    var selected = eventArgs.Values.FirstOrDefault();
                    switch (selected)
                    {
                        case "Plugin DName":
                            value = usercache.Plugin.DName;
                            break;
                        case "Plugin Version":
                            value = usercache.Plugin.Version;
                            break;
                        case "Plugin EAVersion":
                            value = usercache.Plugin.EAVersion;
                            break;
                        case "Plugin ID":
                            value = usercache.Plugin.ID;
                            break;
                        case "Plugin Link":
                            value = usercache.Plugin.Link;
                            break;
                        case "Plugin Notes":
                            value = usercache.Plugin.Description;
                            break;
                    }
                    
                    DiscordInteractionResponseBuilder modal = new();
                    modal.WithCustomId(SelectPluginValueToEdit);
                    modal.WithTitle($"Editing {selected}");
                    if (selected == "Plugin Notes")
                    {
                        modal.AddComponents(
                            new DiscordTextInputComponent(
                                label: selected!, 
                                customId: selected!, 
                                required: true, 
                                value: value,
                                style: DiscordTextInputStyle.Paragraph));
                    }else
                    {
                        modal.AddComponents(
                            new DiscordTextInputComponent(
                                label: selected!, 
                                customId: selected!, 
                                required: true, 
                                value: value,
                                style: DiscordTextInputStyle.Short));
                    }

                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                }
                                
                if (eventArgs.Id is SelectErrorValueToEdit or SelectErrorValueToFinish)
                {
                    var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, SelectErrorValueToEdit);
                    if (usercache == null)
                    {
                        var bd = new DiscordInteractionResponseBuilder();
                        bd.IsEphemeral = true;
                        bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor, or the bot reset during this editor session!", true));
                        await eventArgs.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                        return;
                    }

                    if (eventArgs.Id == SelectErrorValueToFinish)
                    {
                        await FindErrorMessages.SendDbOperationConfirmation(usercache.Error, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, Database.GetError(usercache.Error.ID));
                        Database.EditError(usercache.Error);
                        Program.Cache.RemoveUserAction(eventArgs.User.Id, SelectErrorValueToEdit);
                        await eventArgs.Message.DeleteAsync();
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType
                            .DeferredMessageUpdate);
                        return;
                    }
                    
                    var value = string.Empty;
                    var selected = eventArgs.Values.FirstOrDefault();
                    switch (selected)
                    {
                        case "Error Regex":
                            value = usercache.Error.Regex;
                            break;
                        case "Error Solution":
                            value = usercache.Error.Solution;
                            break;
                        case "Error Description":
                            value = usercache.Error.Description;
                            break;
                    }
                    
                    DiscordInteractionResponseBuilder modal = new();
                    modal.WithCustomId(SelectErrorValueToEdit);
                    modal.WithTitle($"Editing {selected}");
                    modal.AddComponents(
                        new DiscordTextInputComponent(
                            label: selected!, 
                            customId: selected!, 
                            required: true, 
                            value: value,
                            style: DiscordTextInputStyle.Paragraph
                        )
                    );

                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                }
                    
                //===//===//===////===//===//===////===//RPH Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is RphGetQuickInfo) 
                    await cache.RphProcess.SendQuickLogInfoMessage(eventArgs: eventArgs);
                
                if (eventArgs.Id is RphGetDetailedInfo) 
                    await cache.RphProcess.SendDetailedInfoMessage(eventArgs);
                
                if (eventArgs.Id is RphGetAdvancedInfo) 
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
                    new DiscordTextInputComponent(
                        label: "Feedback:", 
                        customId: "feedback", 
                        required: true, 
                        style: DiscordTextInputStyle.Paragraph
                    )
                );

                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
            }
            
            //===//===//===////===//===//===////===//Request Help Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == RequestHelp)
            {
                DiscordInteractionResponseBuilder modal = new();
                modal.WithCustomId(ModalSubmit.RequestHelp);
                modal.WithTitle($"Requesting help!");
                modal.AddComponents(new DiscordTextInputComponent(
                    label: "Explain your issue in detail:",
                    customId: "issueDsc",
                    required: true,
                    style: DiscordTextInputStyle.Paragraph
                ));
        
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
            }
            
            //===//===//===////===//===//===////===//Mark Solved Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == MarkSolved)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Program.Cache.GetCasess().First(x => x.ChannelID.Equals(eventArgs.Channel.Id.ToString()));
                var tmpuser = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);

                if (eventArgs.User.Id.ToString().Equals(ac.OwnerID) || tmpuser.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
                {
                    msg.AddEmbed(BasicEmbeds.Info("Closing case!"));
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
                    await CloseCase.Close(ac);
                    return;
                }
                msg.AddEmbed(BasicEmbeds.Error("You do not own this case!"));
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
            }
            
            //===//===//===////===//===//===////===//Join Case Buttons//===////===//===//===////===//===//===//
            if (eventArgs.Id == JoinCase)
            {
                var ac = Program.Cache.GetCasess().First(x => x.CaseID.Equals(
                    eventArgs.Message.Embeds.First().Description.Split("Case: ")[1].Split("_").First()));
                await Public.AutoHelper.Modules.Case_Functions.JoinCase.Join(ac, eventArgs.User.Id.ToString());
            }

            if (eventArgs.Id == IgnoreRequest)
            {
                var ac = Program.Cache.GetCasess().First(x => x.CaseID.Equals(
                    eventArgs.Message.Embeds.First().Description.Split("Case: ")[1].Split("_").First()));
                var ch = await Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID));
                await ch.SendMessageAsync(BasicEmbeds.Error("__Request Denied!__\r\n>>> This is likely due to you not providing any info, " + 
                "or you have not tried any steps to help yourself.\r\nDirect basic support questions to: <#672541961969729540>", true));
                var chTs = await Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId);
                var tmpmsg = await chTs.GetMessageAsync(ulong.Parse(ac.RequestID));
                await chTs.DeleteMessageAsync(tmpmsg);
                ac.RequestID = null;
                await Database.EditCase(ac);
            }
            
            //===//===//===////===//===//===////===//Open Case Button//===////===//===//===////===//===//===//
            if (eventArgs.Id == OpenCase)
            {
                await eventArgs.Interaction.DeferAsync(true);
                var msg = new DiscordWebhookBuilder();
                if (Database.LoadUsers().FirstOrDefault(x => x.UID == eventArgs.User.Id.ToString())!.Blocked == 1)
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                        $"__You are blacklisted from the bot!__\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!", true)));
                    return;
                }
        
                var findCase = Program.Cache.GetCasess().FirstOrDefault(autocase => autocase.OwnerID.Equals(eventArgs.User.Id.ToString()) && autocase.Solved == 0);
                if (findCase != null)
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error($"__You already have an open case!__\r\nCheck <#{findCase.ChannelID}>", true)));
                    return;
                }

                var newcase = await Public.AutoHelper.Modules.Case_Functions.OpenCase.CreateCase(eventArgs);
                msg.AddEmbed(BasicEmbeds.Success($"Created new case! {newcase.Mention}"));
                await eventArgs.Interaction.EditOriginalResponseAsync(msg);
                await CheckCases.Validate();
            }
        }
        catch (Exception exception)
        {
            await Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}
