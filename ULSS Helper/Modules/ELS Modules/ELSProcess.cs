using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ELS_Modules;

// ReSharper disable InconsistentNaming
public class ELSProcess : SharedLogInfo
{
    public ELSLog log;
    
    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"ELS Version: {log.ElsVersion} - AdvancedHookV installed: {(log.AdvancedHookVFound ? "\u2713" : "X")}"
            }
        };
    }

    public async Task SendQuickLogInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");

        var embed = GetBaseLogInfoEmbed("## Quick ELS.log Info");

        if (targetMessage == null) targetMessage = eventArgs!.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, log);

        if (log.FaultyVcfFile != null) 
        {
            embed.AddField(":red_circle:     Faulty VCF found!", $"Remove `{log.FaultyVcfFile}` from `{log.VcfContainer}`");
            log.ValidElsVcfFiles.Add("\r\n__**A faulty VCF prevented all of these from loading!**__");
        }
        else 
        {
            embed.AddField(":green_circle:     No faulty VCF files detected!", "Seems like ELS loaded fine. If ELS still is not working correctly, make sure to check the asiloader.log as well!");
        }

        if (log.TotalAmountElsModels != null) 
            embed.AddField("Total amount of ELS-enabled models:", log.TotalAmountElsModels.ToString());

        var webhookBuilder = new DiscordWebhookBuilder();
        webhookBuilder.AddEmbed(embed);
        webhookBuilder.AddComponents(
            // ReSharper disable RedundantExplicitParamsArrayCreation
            [
                new DiscordButtonComponent(DiscordButtonStyle.Primary, ComponentInteraction.ElsGetDetailedInfo, "More Info", false, new DiscordComponentEmoji("❗")),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, ComponentInteraction.ElsQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
            ]
        );

        DiscordMessage sentMessage;
        if (context != null)
            sentMessage = await context.EditResponseAsync(webhookBuilder);
        else if (eventArgs.Id == ComponentInteraction.ElsGetQuickInfo)
        {
            var responseBuilder = new DiscordInteractionResponseBuilder(webhookBuilder);
            await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
            sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        }
        else
            sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
            
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this));
    }


    public async Task SendDetailedInfoMessage(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var validVcFiles = "\r\n- " + string.Join(", ", log.ValidElsVcfFiles);
        var invalidVcFiles = "\r\n- " + string.Join("\r\n- ", log.InvalidElsVcfFiles);
        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        
        var embed = GetBaseLogInfoEmbed("## Detailed ELS.log Info");
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            embed.AddField(field.Name, field.Value, field.Inline);
        }

        var buttonComponents = new DiscordComponent[]
        {
            new DiscordButtonComponent(
                DiscordButtonStyle.Secondary,
                ComponentInteraction.ElsGetQuickInfo,
                "Back to Quick Info", 
                false,
                new DiscordComponentEmoji("⬅️")
            ),
            new DiscordButtonComponent(
                DiscordButtonStyle.Danger, 
                ComponentInteraction.ElsDetailedSendToUser, 
                "Send To User", 
                false,
                new DiscordComponentEmoji("📨")
            ),
        };
        
        if (validVcFiles.Length >= 1024 || invalidVcFiles.Length >= 1024)
        {
            await eventArgs.Interaction.DeferAsync(true);
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many VCFs to display in a single message.", true);
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":green_circle:     **Valid VCFs:**",
                Description = validVcFiles,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Invalid VCFs:**",
                Description = invalidVcFiles,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflowBuilder = new DiscordWebhookBuilder();
            overflowBuilder.AddEmbed(embed);
            if (validVcFiles.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (invalidVcFiles.Length != 0) overflowBuilder.AddEmbed(embed3);
            // ReSharper disable RedundantExplicitParamsArrayCreation
            overflowBuilder.AddComponents(buttonComponents);
            var sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflowBuilder);
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this)); 
            return;
        }

        if (log.VcfContainer != null)
            embed.AddField("VCF Container Location:", log.VcfContainer);
        
        if (log.ValidElsVcfFiles.Count > 0)
            embed.AddField(":green_circle:     Valid VCFs:", validVcFiles, true);
        
        if (log.InvalidElsVcfFiles.Count > 0) 
            embed.AddField(":orange_circle:     Invalid VCFs:", invalidVcFiles, true);
        
        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);
        responseBuilder.AddComponents(buttonComponents);
        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this)); 
    }

    public async Task SendMessageToUser(ComponentInteractionCreatedEventArgs eventArgs)
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
        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Sent!")));
        await eventArgs.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(eventArgs.Channel);
    }
}