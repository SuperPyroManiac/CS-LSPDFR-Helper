using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Messages.ModifiedProperties;

namespace LSPDFR_Helper.EventManagers;

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
        CustomIds.RphGetQuickInfo,
        CustomIds.RphGetErrorInfo,
        CustomIds.RphGetPluginInfo,
        CustomIds.RphSendToUser,
        CustomIds.ElsGetQuickInfo,
        CustomIds.ElsGetDetailedInfo,
        CustomIds.ElsQuickSendToUser,
        CustomIds.ElsDetailedSendToUser,
        CustomIds.AsiGetQuickInfo,
        CustomIds.AsiGetDetailedInfo,
        CustomIds.AsiQuickSendToUser,
        CustomIds.AsiDetailedSendToUser,
        CustomIds.ShvdnGetQuickInfo,
        CustomIds.ShvdnGetDetailedInfo,
        CustomIds.ShvdnQuickSendToUser,
        CustomIds.ShvdnDetailedSendToUser
    ];
    
    public static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreatedEventArgs eventArgs)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);

        if ( CacheEventIds.Any(eventId => eventId == eventArgs.Id) )
        {//Handle cached interaction events here.
            var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
            
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
                var selectComp = (DiscordSelectComponent) eventArgs.Message.Components
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
                    Program.Cache.UpdatePlugins(DbManager.GetPlugins());
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
                    Program.Cache.UpdateErrors(DbManager.GetErrors());
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
        }
        //Handle non cached interaction events here.
    }
}