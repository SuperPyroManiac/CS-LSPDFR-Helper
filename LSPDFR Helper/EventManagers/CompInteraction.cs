using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;

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
        CustomIds.RphGetDetailedInfo,
        CustomIds.RphGetAdvancedInfo,
        CustomIds.RphQuickSendToUser,
        CustomIds.RphDetailedSendToUser,
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
            //TODO: var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
            
            //===//===//===////===//===//===////===//Editor Dropdowns//===////===//===//===////===//===//===//
            
            if (eventArgs.Id is CustomIds.SelectPluginValueToEdit or CustomIds.SelectPluginValueToFinish)
            {
                var usercache = Program.Cache.GetUserAction(eventArgs.User.Id, CustomIds.SelectPluginValueToEdit);
                if (usercache == null)
                {
                    var bd = new DiscordInteractionResponseBuilder();
                    bd.IsEphemeral = true;
                    bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor!", true));
                    await eventArgs.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                    return;
                }

                if (eventArgs.Id == CustomIds.SelectPluginValueToFinish)
                {
                    //await FindPluginMessages.SendDbOperationConfirmation(usercache.Plugin, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, Database.GetPlugin(usercache.Plugin.Name));
                    DbManager.EditPlugin(usercache.Plugin);
                    Program.Cache.RemoveUserAction(eventArgs.User.Id, CustomIds.SelectPluginValueToEdit);
                    await eventArgs.Message.DeleteAsync();
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                    Program.Cache.UpdatePlugins(DbManager.GetPlugins());
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
                        value = usercache.Plugin.EaVersion;
                        break;
                    case "Plugin ID":
                        value = usercache.Plugin.Id.ToString();
                        break;
                    case "Plugin Link":
                        value = usercache.Plugin.Link;
                        break;
                    case "Plugin Notes":
                        value = usercache.Plugin.Description;
                        break;
                }
                    
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
                    bd.AddEmbed(BasicEmbeds.Error("There was a problem!\r\n>>> You are not the original editor, or the bot reset during this editor session!", true));
                    await eventArgs.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource, bd);
                    return;
                }

                if (eventArgs.Id == CustomIds.SelectErrorValueToFinish)
                {
                    //await FindErrorMessages.SendDbOperationConfirmation(usercache.Error, DbOperation.UPDATE, eventArgs.Interaction.ChannelId, eventArgs.Interaction.User.Id, Database.GetError(usercache.Error.ID));
                    DbManager.EditError(usercache.Error);
                    Program.Cache.RemoveUserAction(eventArgs.User.Id, CustomIds.SelectErrorValueToEdit);
                    await eventArgs.Message.DeleteAsync();
                    await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                    Program.Cache.UpdateErrors(DbManager.GetErrors());
                    return;
                }
                    
                var value = string.Empty;
                var selected = eventArgs.Values.FirstOrDefault();
                switch (selected)
                {
                    case "Error Pattern":
                        value = usercache.Error.Pattern;
                        break;
                    case "Error Solution":
                        value = usercache.Error.Solution;
                        break;
                    case "Error Description":
                        value = usercache.Error.Description;
                        break;
                    case "Error String Match":
                        value = usercache.Error.StringMatch.ToString();
                        break;
                }
                    
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