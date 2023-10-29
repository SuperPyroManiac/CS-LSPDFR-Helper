﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using ULSS_Helper.Modules.LogAnalyzer;
using ULSS_Helper.Modules.Messages;
using static ULSS_Helper.Modules.Messages.RphLogAnalysisMessages;

namespace ULSS_Helper.Modules;

internal class ContextManager : ApplicationCommandModule
{

    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    [SlashRequirePermissions(Permissions.ManageMessages)]
    public static async Task OnMenuSelect(ContextMenuContext e)
    {
        await e.DeferAsync(true);

        if (e.Member.Roles.All(role => role.Id != Settings.GetTSRole()))//TODO: Proper permissions setup
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(BasicEmbeds.Error("You do not have permission for this!"));
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }

        try
        {
            await LogAnalyzerManager.ProcessAttachments(e);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
    
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id is "send" or "send2") await SendMessageToUser(e);
        if (e.Id == "info") await SendDetailedInfoMessage(e);
    }
}