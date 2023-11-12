using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ELS_Modules;

internal class ELSProcess : SharedLogInfo
{
    internal ELSLog log;

    internal async Task SendQuickLogInfoMessage(ContextMenuContext? context=null, ComponentInteractionCreateEventArgs? eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");

        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Quick ELS.log Info");

        DiscordMessage targetMessage = context?.TargetMessage ?? eventArgs.Message;
        ProcessCache cache = Program.Cache.GetProcessCache(targetMessage.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage);

        if (log.FaultyVcfFile != null) 
        {
            embed.AddField($":red_circle:     Faulty VCF found!", $"Remove `{log.FaultyVcfFile}` from `{log.VcfContainer}`");
        }
        else 
        {
            embed.AddField($":green_circle:     No faulty VCF files detected!", "Seems like ELS loaded fine.");
        }

        if (log.TotalAmountElsModels != null) 
            embed.AddField("Total amount of ELS-enabled models:", log.TotalAmountElsModels.ToString() ?? "0");

        DiscordWebhookBuilder message = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
                new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.ElsGetDetailedInfo, "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ElsQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                }
            );

        DiscordMessage? sentMessage;
        if (context != null)
            sentMessage = await context.EditResponseAsync(message);
        else
            sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(message);
            
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this));
    }


    internal async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        string validVcFiles = "\r\n- " + string.Join(", ", log.ValidElsVcfFiles);
        string invalidVcFiles = "\r\n- " + string.Join("\r\n- ", log.InvalidElsVcfFiles);
        ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);
        
        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Detailed ELS.log Info");
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            embed.AddField(field.Name, field.Value, field.Inline);
        }
        
        if (validVcFiles.Length >= 1024 || invalidVcFiles.Length >= 1024)
        {
            await eventArgs.Interaction.DeferAsync(true);
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many VCFs to display in a single message.", true);
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":green_circle:     **Valid VCFs:**",
                Description = validVcFiles,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Invalid VCFs:**",
                Description = invalidVcFiles,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (validVcFiles.Length != 0) overflow.AddEmbed(embed2);
            if (invalidVcFiles.Length != 0) overflow.AddEmbed(embed3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ElsDetailedSendToUser, "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            DiscordMessage? sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflow);
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
            return;
        }

        if (log.VcfContainer != null)
            embed.AddField("VCF Container Location:", log.VcfContainer);
        
        if (log.ValidElsVcfFiles.Count > 0)
            embed.AddField(":green_circle:     Valid VCFs:", validVcFiles, true);
        
        if (log.InvalidElsVcfFiles.Count > 0) 
            embed.AddField(":orange_circle:     Invalid VCFs:", invalidVcFiles, true);
            
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ElsDetailedSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
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

    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"ELS Version: {log.ElsVersion} - AdvancedHookV installed: {(log.AdvancedHookVFound ? "\u2713" : "X")}"
            }
        };
    }
}