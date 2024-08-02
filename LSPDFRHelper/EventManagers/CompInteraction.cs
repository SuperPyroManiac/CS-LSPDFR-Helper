using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.AutoHelper;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Messages.ModifiedProperties;
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
                                                                "or you have not tried any steps to help yourself.\r\nDirect basic support questions to: <#672541961969729540>"));
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
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}