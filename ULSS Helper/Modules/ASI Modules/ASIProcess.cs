using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ASI_Modules;

// ReSharper disable InconsistentNaming
internal class ASIProcess : SharedLogInfo
{
    internal ASILog log;
    
    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Loaded ASIs: {log.LoadedAsiFiles.Count} - Failed ASIs: {log.FailedAsiFiles.Count}"
            }
        };
    }
    internal async Task SendQuickLogInfoMessage(ContextMenuContext context=null, ComponentInteractionCreateEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");

        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Quick ASI.log Info");

        DiscordMessage targetMessage = context?.TargetMessage ?? eventArgs.Message;
        ProcessCache cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, log);

        if (log.FailedAsiFiles.Count != 0) 
        {
            embed.AddField(":red_circle:     Some ASIs failed to load!", "Select `More Info` for details!");
        }
        else 
        {
            embed.AddField(":green_circle:     No faulty ASI files detected!", "Seems like everything loaded fine.");
        }

        DiscordWebhookBuilder message = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
	            // ReSharper disable RedundantExplicitParamsArrayCreation
	            new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.AsiGetDetailedInfo, "More Info", false, new DiscordComponentEmoji(Program.Settings.Env.MoreInfoBtnEmojiId)),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.AsiQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                }
            );

        DiscordMessage sentMessage;
        if (context != null)
            sentMessage = await context.EditResponseAsync(message);
        else
            sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(message);
            
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this));
    }
    
    internal async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        var loadedAsiFilesList = "\r\n- " + string.Join("\r\n- ", log.LoadedAsiFiles);
        var failedAsiFilesList = "\r\n- " + string.Join("\r\n- ", log.FailedAsiFiles);
        ProcessCache cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        
        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Detailed ASI.log Info");
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains("failed to load"))
            {
                embed.AddField(field.Name, field.Value, field.Inline);
            }
        }
        
        if (loadedAsiFilesList.Length >= 1024 || failedAsiFilesList.Length >= 1024)
        {
            await eventArgs.Interaction.DeferAsync(true);
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many ASIs to display in a single message.", true);
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":green_circle:     **Loaded ASIs:**",
                Description = loadedAsiFilesList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Failed ASIs:**",
                Description = failedAsiFilesList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (loadedAsiFilesList.Length != 0) overflow.AddEmbed(embed2);
            if (failedAsiFilesList.Length != 0) overflow.AddEmbed(embed3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.AsiDetailedSendToUser, "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            DiscordMessage sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflow);
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
            return;
        }
        
        if (log.LoadedAsiFiles.Count > 0)
            embed.AddField(":green_circle:     Loaded ASIs:", loadedAsiFilesList, true);
        
        if (log.FailedAsiFiles.Count > 0) 
            embed.AddField(":red_circle:     Failed ASIs:", failedAsiFilesList, true);
            
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.AsiDetailedSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
        }));
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
    }

    internal async Task SendMessageToUser(ComponentInteractionCreateEventArgs eventArgs)
    {
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseLogInfoEmbed(eventArgs.Message.Embeds[0].Description);
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            newEmb.AddField(field.Name, field.Value, field.Inline);
        }
        newEmb = RemoveTsViewFields(newEmb);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(eventArgs.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithReply(log.MsgId, true);
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Sent!")));
        await eventArgs.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(eventArgs.Channel);
    }
}