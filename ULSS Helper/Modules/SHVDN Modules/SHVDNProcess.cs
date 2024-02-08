using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.SHVDN_Modules;

// ReSharper disable once InconsistentNaming
internal class SHVDNProcess : SharedLogInfo
{
        internal SHVDNLog log;
    
    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Problems: {log.Scripts.Count}"
            }
        };
    }
    internal async Task SendQuickLogInfoMessage(ContextMenuContext context=null, ComponentInteractionCreateEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");

        var embed = GetBaseLogInfoEmbed("## Quick SHVDN.log Info");

        var targetMessage = context?.TargetMessage ?? eventArgs.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, log);

        if (log.Scripts.Count != 0) 
        {
            embed.AddField(":red_circle:     Some scripts have issues!", "Select `More Info` for details!");
        }
        else 
        {
            embed.AddField(":green_circle:     No faulty script files detected!", "Seems like everything loaded fine.");
        }

        var webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
	            // ReSharper disable RedundantExplicitParamsArrayCreation
                [
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.ShvdnGetDetailedInfo, "More Info", false, new DiscordComponentEmoji("❗")),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ShvdnQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                ]
            );

        DiscordMessage sentMessage;
        if (context != null)
            sentMessage = await context.EditResponseAsync(webhookBuilder);
        else if (eventArgs.Id == ComponentInteraction.ShvdnGetQuickInfo)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder(webhookBuilder);
            await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
            sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        }
        else
            sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
            
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this));
    }
    
    internal async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        var scriptsList = "\r\n- " + string.Join("\r\n- ", log.Scripts);
        var missingDependsList = "\r\n- " + string.Join("\r\n- ", log.MissingDepends);
        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        
        var embed = GetBaseLogInfoEmbed("## Detailed SHVDN.log Info");
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains("Some scripts have issues"))
            {
                embed.AddField(field.Name, field.Value, field.Inline);
            }
        }
        
        var buttonComponents = new DiscordComponent[]
        {
            new DiscordButtonComponent(
                ButtonStyle.Secondary,
                ComponentInteraction.ShvdnGetQuickInfo,
                "Back to Quick Info", 
                false,
                new DiscordComponentEmoji("⬅️")
            ),
            new DiscordButtonComponent(
                ButtonStyle.Danger, 
                ComponentInteraction.ShvdnDetailedSendToUser, 
                "Send To User", 
                false,
                new DiscordComponentEmoji("📨")
            ),
        };
        
        if (scriptsList.Length >= 1024 || missingDependsList.Length >= 1024)
        {
            await eventArgs.Interaction.DeferAsync(true);
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many Scripts to display in a single message.", true);
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Script:**",
                Description = scriptsList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Missing Depend:**",
                Description = missingDependsList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflowBuilder = new DiscordWebhookBuilder();
            overflowBuilder.AddEmbed(embed);
            if (scriptsList.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (missingDependsList.Length != 0) overflowBuilder.AddEmbed(embed3);
            // ReSharper disable RedundantExplicitParamsArrayCreation
            overflowBuilder.AddComponents(buttonComponents);
            var sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflowBuilder);
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
            return;
        }
        
        if (log.Scripts.Count > 0)
            embed.AddField(":orange_circle:     Script:", scriptsList, true);
        
        if (log.MissingDepends.Count > 0) 
            embed.AddField(":red_circle:     Missing Depend:", missingDependsList, true);
        
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);
        responseBuilder.AddComponents(buttonComponents);
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
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