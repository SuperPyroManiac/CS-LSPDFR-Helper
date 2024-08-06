using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.AutoHelper;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Messages.ModifiedProperties;
using LSPDFRHelper.Functions.Processors.ASI;
using LSPDFRHelper.Functions.Processors.ELS;
using LSPDFRHelper.Functions.Processors.RPH;
using LSPDFRHelper.Functions.Processors.XML;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.EventManagers;

public static class CompInteraction
{
    private static readonly List<string> CacheEventIds =
    [
        CustomIds.SelectAttachmentForAnalysis,
        CustomIds.SelectIdForRemoval,
        CustomIds.SelectPluginValueToEdit,
        CustomIds.SelectPluginValueToFinish,
        CustomIds.SelectErrorValueToEdit,
        CustomIds.SelectErrorValueToFinish,
        CustomIds.SelectServerValueToEdit,
        CustomIds.SelectServerValueToFinish,
        CustomIds.RphGetQuickInfo,
        CustomIds.RphGetErrorInfo,
        CustomIds.RphGetPluginInfo,
        CustomIds.RphSendToUser,
        CustomIds.ElsSendToUser,
        CustomIds.AsiSendToUser,
        CustomIds.ShvdnGetQuickInfo,
        CustomIds.ShvdnGetDetailedInfo,
        CustomIds.ShvdnQuickSendToUser,
        CustomIds.ShvdnDetailedSendToUser
    ];
    
    public static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreatedEventArgs eventArgs)
    {
        try
        {
            while ( !Program.IsStarted ) await Task.Delay(500);

            if ( CacheEventIds.Any(eventId => eventId == eventArgs.Id) )
            {//Handle cached interaction events here.
                var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
            
                //===//===//===////===//===//===////===//ASI Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is CustomIds.AsiSendToUser) 
                    await cache.AsiProcessor.SendMessageToUser(eventArgs);
            
                //===//===//===////===//===//===////===//ELS Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is CustomIds.ElsSendToUser) 
                    await cache.ElsProcessor.SendMessageToUser(eventArgs);
            
                //===//===//===////===//===//===////===//RPH Buttons//===////===//===//===////===//===//===//
                if (eventArgs.Id is CustomIds.RphGetQuickInfo) 
                    await cache.RphProcessor.SendQuickInfoMessage(eventArgs: eventArgs);
                
                if (eventArgs.Id is CustomIds.RphGetErrorInfo) 
                    await cache.RphProcessor.UpdateToErrorMessage(eventArgs);
                
                if (eventArgs.Id is CustomIds.RphGetPluginInfo) 
                    await cache.RphProcessor.UpdateToPluginMessage(eventArgs);
                
                if (eventArgs.Id is CustomIds.RphSendToUser) 
                    await cache.RphProcessor.SendMessageToUser(eventArgs);
                
                //===//===//===////===//===//===////===//Multi Selector//===////===//===//===////===//===//===//
                if (eventArgs.Id.Equals(CustomIds.SelectAttachmentForAnalysis))
                {
                    var selectedValue = eventArgs.Values.FirstOrDefault();
                    var ids = selectedValue!.Split("&");
                    var messageId = ulong.Parse(ids[0]);
                    var targetAttachmentId = ulong.Parse(ids[1]);
                    var message = await eventArgs.Channel.GetMessageAsync(messageId);
                    var targetAttachment = message.Attachments.FirstOrDefault(attachment => attachment.Id == targetAttachmentId);
                    
                    if (targetAttachment!.FileName!.Contains("RagePluginHook"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        RphProcessor rphProcess;
                        if (ProcessCache.IsCacheUsagePossible("RagePluginHook", cache, message.Attachments.ToList()))
                            rphProcess = cache.RphProcessor;
                        else
                        {
                            rphProcess = new RphProcessor();
                            rphProcess.Log = await RPHValidater.Run(targetAttachment.Url);
                            rphProcess.Log.MsgId = cache.OriginalMessage.Id;
                            //ProxyCheck.Run(rphProcess.log, Program.Cache.GetUser(eventArgs.Message.Author!.Id.ToString()), eventArgs.Message);
                            Program.Cache.SaveProcess(messageId: eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, rphProcess));
                        }

                        await rphProcess.SendQuickInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ELS"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        ELSProcessor elsProcess;
                        if (ProcessCache.IsCacheUsagePossible("ELS", cache, message.Attachments.ToList()))
                            elsProcess = cache.ElsProcessor;
                        else
                        {
                            elsProcess = new ELSProcessor();
                            elsProcess.Log = await ELSValidater.Run(targetAttachment.Url);
                            elsProcess.Log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, elsProcess));
                        }

                        await elsProcess.SendQuickInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("asiloader"))
                    {
                        await eventArgs.Interaction.DeferAsync(true);
                        ASIProcessor asiProcess;
                        if (ProcessCache.IsCacheUsagePossible("ASI", cache, message.Attachments.ToList()))
                            asiProcess = cache.AsiProcessor;
                        else 
                        {
                            asiProcess = new ASIProcessor();
                            asiProcess.Log = await ASIValidater.Run(targetAttachment.Url);
                            asiProcess.Log.MsgId = cache.OriginalMessage.Id;
                            Program.Cache.SaveProcess(eventArgs.Message.Id, new ProcessCache(eventArgs.Message.Interaction, cache.OriginalMessage, asiProcess));
                        }
                        
                        await asiProcess.SendQuickInfoMessage(eventArgs: eventArgs);
                        return;
                    }
                    if (targetAttachment.FileName.Contains("ScriptHookVDotNet"))
                    {
                        //TODO
                    }
                    if ( targetAttachment.FileName.EndsWith(".xml") || targetAttachment.FileName.EndsWith(".meta") )
                    {
                        var xmlData = await XmlValidator.Run(targetAttachment.Url);
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Ts($"## __XML Validator__{BasicEmbeds.AddBlanks(35)}", null).AddField(targetAttachment.FileName, $">>> ```xml\r\n{xmlData}\r\n```")));
                        return;
                    }
                }
            
                //===//===//===////===//===//===////===//Remove Errors//===////===//===//===////===//===//===//
                if (eventArgs.Id.Equals(CustomIds.SelectIdForRemoval))
                {
                    // Get the current SelectComponent from the Message that the user interacted with
                    var selectComp = (DiscordSelectComponent) eventArgs.Message.Components!
                        .FirstOrDefault(compRow => compRow.Components.Any(comp => comp.CustomId == CustomIds.SelectIdForRemoval))
                        ?.Components!.FirstOrDefault(comp => comp.CustomId == CustomIds.SelectIdForRemoval);
                    // Get a list of all components (like buttons) except for the SelectComponent so we can rebuild the list of components later after modifying the SelectComponent
                    var allComponentsExceptSelect = eventArgs.Message.Components
                        .FirstOrDefault(compRow => compRow.Components.All(comp => comp.CustomId != CustomIds.SelectIdForRemoval))?.Components;
                    
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
                                customId: CustomIds.SelectIdForRemoval,
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
                        if (new Regex(@"ID:\s?\d+\D+").IsMatch(fieldName!))
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
                if (eventArgs.Id is CustomIds.SelectPluginValueToEdit or CustomIds.SelectPluginValueToFinish)
                {
                    var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, CustomIds.SelectPluginValueToEdit);
                    if (usercache == null)
                    {
                        var bd = new DiscordInteractionResponseBuilder();
                        bd.IsEphemeral = true;
                        bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor!"));
                        await eventArgs.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                        return;
                    }

                    if (eventArgs.Id == CustomIds.SelectPluginValueToFinish)
                    {
                        await FindPluginMessages.SendDbOperationConfirmation(usercache.Plugin, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, DbManager.GetPlugin(usercache.Plugin.Name));
                        DbManager.EditPlugin(usercache.Plugin);
                        Program.Cache.RemoveUserAction(eventArgs.User.Id, CustomIds.SelectPluginValueToEdit);
                        await eventArgs.Message.DeleteAsync();
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                        return;
                    }
                    
                    var selected = eventArgs.Values.FirstOrDefault();
                    var value = selected switch
                    {
                        "Plugin DName" => usercache.Plugin.DName,
                        "Plugin Version" => usercache.Plugin.Version,
                        "Plugin EAVersion" => usercache.Plugin.EaVersion,
                        "Plugin Id" => usercache.Plugin.Id.ToString(),
                        "Plugin AuthorId" => usercache.Plugin.AuthorId.ToString(),
                        "Plugin Link" => usercache.Plugin.Link,
                        "Plugin Notes" => usercache.Plugin.Description,
                        "Plugin Announce" => usercache.Plugin.Announce.ToString(),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    DiscordInteractionResponseBuilder modal = new();
                    modal.WithCustomId(CustomIds.SelectPluginValueToEdit);
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
            
                if (eventArgs.Id is CustomIds.SelectErrorValueToEdit or CustomIds.SelectErrorValueToFinish)
                {
                    var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, CustomIds.SelectErrorValueToEdit);
                    if (usercache == null)
                    {
                        var bd = new DiscordInteractionResponseBuilder();
                        bd.IsEphemeral = true;
                        bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor, or the bot reset during this editor session!"));
                        await eventArgs.Interaction.CreateResponseAsync(
                            DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                        return;
                    }

                    if (eventArgs.Id == CustomIds.SelectErrorValueToFinish)
                    {
                        await FindErrorMessages.SendDbOperationConfirmation(usercache.Error, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, DbManager.GetError(usercache.Error.Id.ToString()));
                        DbManager.EditError(usercache.Error);
                        Program.Cache.RemoveUserAction(eventArgs.User.Id, CustomIds.SelectErrorValueToEdit);
                        await eventArgs.Message.DeleteAsync();
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                        return;
                    }
                    
                    var selected = eventArgs.Values.FirstOrDefault();
                    var value = selected switch
                    {
                        "Error Pattern" => usercache.Error.Pattern,
                        "Error Solution" => usercache.Error.Solution,
                        "Error Description" => usercache.Error.Description,
                        "Error String Match" => usercache.Error.StringMatch.ToString(),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    DiscordInteractionResponseBuilder modal = new();
                    modal.WithCustomId(CustomIds.SelectErrorValueToEdit);
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
            
                if (eventArgs.Id is CustomIds.SelectServerValueToEdit or CustomIds.SelectServerValueToFinish)
                {
                    var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, CustomIds.SelectServerValueToEdit);
                    if (usercache == null)
                    {
                        var bd = new DiscordInteractionResponseBuilder();
                        bd.IsEphemeral = true;
                        bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor, or the bot reset during this editor session!"));
                        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                        return;
                    }

                    if (eventArgs.Id == CustomIds.SelectServerValueToFinish)
                    {
                        DbManager.EditServer(usercache.Server);
                        Program.Cache.RemoveUserAction(eventArgs.User.Id, CustomIds.SelectServerValueToEdit);
                        await AutoHelper.UpdateMainAhMessage(eventArgs.Guild.Id);
                        await AutoHelper.UpdateAhMonitor(eventArgs.Guild.Id);
                        await Task.Delay(500);
                        Task.WaitAll(eventArgs.Message.DeleteAsync(), eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate));
                        return;
                    }
                    
                    var selected = eventArgs.Values.FirstOrDefault();
                    var value = selected switch
                    {
                        "AhCh" => usercache.Server.AutoHelperChId.ToString(),
                        "MonitorCh" => usercache.Server.MonitorChId.ToString(),
                        "ManagerRole" => usercache.Server.ManagerRoleId.ToString(),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    DiscordInteractionResponseBuilder modal = new();
                    modal.WithCustomId(CustomIds.SelectServerValueToEdit);
                    modal.WithTitle($"Editing {selected}");
                    modal.AddComponents(
                        new DiscordTextInputComponent(
                            label: selected!, 
                            customId: selected!, 
                            required: true, 
                            value: value,
                            style: DiscordTextInputStyle.Short
                        )
                    );

                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
                }
            }
            //Handle non cached interaction events here.

            //===//===//===////===//===//===////===//AutoHelper Buttons//===////===//===//===////===//===//===//
            if (eventArgs.Id == CustomIds.OpenCase)
            {
                await eventArgs.Interaction.DeferAsync(true);
                var msg = new DiscordWebhookBuilder();
                if (Program.Cache.GetUser(eventArgs.User.Id).Blocked)
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error("__You are blacklisted from the bot!__\r\n>>> Contact the devs at https://dsc.PyrosFun.com if you think this is an error!")));
                    return;
                }

                var findCase = Program.Cache.GetCases().FirstOrDefault(autocase => autocase.OwnerId.Equals(eventArgs.User.Id) && !autocase.Solved);
                if (findCase != null)
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error($"__You already have an open case!__\r\n> Check <#{findCase.ChannelId}>")));
                    return;
                }
                
                if (!Program.Cache.GetServer(eventArgs.Guild.Id).AutoHelperChId.Equals(eventArgs.Channel.Id))
                {
                    await eventArgs.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error($"__Server setup incorrectly!__\r\n>>> Please report this to the server staff!\r\n*An admin needs to run `/setup` and ensure the channel id's are correct!*")));
                    return;
                }

                var newCase = await OpenCase.CreateCase(eventArgs.User.Id, eventArgs.Guild.Id);
                msg.AddEmbed(BasicEmbeds.Success($"__Created new case!__\r\n> {newCase.Mention}"));
                await eventArgs.Interaction.EditOriginalResponseAsync(msg);
            }

            if (eventArgs.Id == CustomIds.MarkSolved)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Program.Cache.GetCases().First(x => x.ChannelId.Equals(eventArgs.Channel.Id));

                if (eventArgs.User.Id.Equals(ac.OwnerId) || await Program.Cache.GetUser(eventArgs.User.Id).IsManager(eventArgs.Guild.Id))
                {
                    msg.AddEmbed(BasicEmbeds.Info("Closing case!", false));
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
                    await CloseCase.Close(ac);
                    return;
                }
                msg.AddEmbed(BasicEmbeds.Error("__You do not own this case!__"));
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
            }

            if (eventArgs.Id == CustomIds.JoinCase)
            {
                var ac = Program.Cache.GetCases().First(x => x.CaseId.Equals(eventArgs.Message.Embeds.First().Description!.Split("Case: ")[1].Split("_").First()));
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                if ( Program.Cache.GetUser(eventArgs.User.Id).Blocked )
                {
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        msg.AddEmbed(BasicEmbeds.Error("__You are blacklisted from the bot!__\r\n>>> Contact the devs at https://dsc.PyrosFun.com if you think this is an error!")));
                    return;
                }

                if ( !await JoinCase.Join(ac, eventArgs.User.Id, eventArgs.Guild.Id) )
                {
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg.AddEmbed(BasicEmbeds.Error(
                        $"__Already joined!__\r\n>>> You have already joined case: `{ac.CaseId}`!\r\nSee: <#{ac.ChannelId}>")));
                    return;
                }
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    msg.AddEmbed(BasicEmbeds.Success($"__Case Joined!__\r\n> <#{ac.ChannelId}>")));
            }

            if (eventArgs.Id == CustomIds.IgnoreRequest)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                if ( await Program.Cache.GetUser(eventArgs.User.Id).IsManager(eventArgs.Guild.Id) )
                {
                    var ac = Program.Cache.GetCases().First(x => x.CaseId.Equals(eventArgs.Message.Embeds.First().Description!.Split("Case: ")[1].Split("_").First()));
                    var ch = await Program.Client.GetChannelAsync(ac.ChannelId);
                    await ch.SendMessageAsync(BasicEmbeds.Error("__Request Denied!__\r\n>>> This is likely due to you not providing any info, " +
                                                                "or you have not tried any steps to help yourself."));
                    var chTs = await Program.Client.GetChannelAsync(Program.Cache.GetServer(eventArgs.Guild.Id).MonitorChId);
                    await chTs.DeleteMessageAsync(await chTs.GetMessageAsync(ac.RequestId));
                    ac.RequestId = 0;
                    DbManager.EditCase(ac);
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                        msg.AddEmbed(BasicEmbeds.Success($"__Request Ignored!__\r\n> <#{ac.ChannelId}>")));
                    return;
                }
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    msg.AddEmbed(BasicEmbeds.Error("__No Permission!__\r\n> Only server TS can use this!")));
            }

            if (eventArgs.Id == CustomIds.RequestHelp)
            {
                DiscordInteractionResponseBuilder modal = new();
                modal.WithCustomId(CustomIds.RequestHelp);
                modal.WithTitle("Requesting help!");
                modal.AddComponents(new DiscordTextInputComponent(
                    label: "Explain your issue in detail:",
                    customId: "issueDsc",
                    required: true,
                    style: DiscordTextInputStyle.Paragraph
                ));

                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
            }
            
            //===//===//===////===//===//===////===//Setup Buttons//===////===//===//===////===//===//===//
            
            if (eventArgs.Id == CustomIds.SelectSetupInfo)
            {
                var msg = new DiscordInteractionResponseBuilder()
                    .AddEmbed(BasicEmbeds.Info(
                        "__Setup Info__" +
                        "\r\n> - Choose a manager role you want to have access to the support commands." +
                        "\r\n> - Choose a channel for the AutoHelper. Recommend creating an empty where people cannot type. The AutoHelper creates private threads in this channel that are used for the individual cases." +
                        "\r\n> - Choose a channel for the AutoHelper Monitor. This shows all open cases as well as is where messages are posted when the request help button is used. Recommend setting this to a new channel where people cannot type." +
                        "\r\n> - Run `/setup` and assign the channel id's and role id if applicable."))
                    .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.SelectCommandInfo, "Command Info", false, new DiscordComponentEmoji("📋")));
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, msg);
            }
            
            if (eventArgs.Id == CustomIds.SelectCommandInfo)
            {
                var msg = new DiscordInteractionResponseBuilder()
                    .AddEmbed(BasicEmbeds.Info(
                        "__Command Info__" +
                        "\r\n> Right click context menu apps:" +
                        "\r\n> - `Validate Log`: This will process the selected logs. *(Public)*" +
                        "\r\n> - `Validate XML`: This will parse XML and META files. *(Public)*" +
                        "\r\n> Console commands:" +
                        "\r\n> - `/setup`: Change bot settings. *(Server Admins)*" +
                        "\r\n> - `/ToggleAH`: Enables or disables the AutoHelper. *(Manager Role)*" +
                        "\r\n> - `/Case Find <User>`: Finds the last 25 AutoHelper cases from a user. *(Manager Role)*" +
                        "\r\n> - `/Case Close <Case/All>`: Closes the specified case. Can put `all` to close all cases. *(Manager Role)*" +
                        "\r\n> - `/CheckPlugin <Plugin>`: View information on any plugin in our DB. *(Public)*" +
                        "\r\n> - `/Case Join <Case>`: Join an open AutoHelper case. *(Public)*" +
                        "\r\n> **You can adjust these by setting up the integration permissions in your server settings!**"))
                    .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.SelectSetupInfo, "Setup Info", false, new DiscordComponentEmoji("🛠️")));
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, msg);
            }
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}