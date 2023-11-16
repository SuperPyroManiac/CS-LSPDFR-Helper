using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.SHVDN_Modules;

internal class SHVDNProcess : SharedLogInfo
{
        internal SHVDNLog log;
    
    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"Problems: {log.Scripts.Count}"
            }
        };
    }
    internal async Task SendQuickLogInfoMessage(ContextMenuContext? context=null, ComponentInteractionCreateEventArgs? eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");

        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Quick SHVDN.log Info");

        DiscordMessage targetMessage = context?.TargetMessage ?? eventArgs.Message;
        ProcessCache cache = Program.Cache.GetProcessCache(targetMessage.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage, log.ElapsedTime);

        if (log.Scripts.Count != 0) 
        {
            embed.AddField($":red_circle:     Some scripts have issues!", "Select `More Info` for details!");
        }
        else 
        {
            embed.AddField($":orange_circle:     No faulty script files detected!", "Seems like everything loaded fine.");
        }

        DiscordWebhookBuilder message = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
                new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.ShvdnGetDetailedInfo, "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ShvdnQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
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
        var ScriptsList = "\r\n- " + string.Join("\r\n- ", log.Scripts);
        var MissingDependsList = "\r\n- " + string.Join("\r\n- ", log.MissingDepends);
        ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);
        
        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Detailed SHVDN.log Info");
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains("Some scripts have issues"))
            {
                embed.AddField(field.Name, field.Value, field.Inline);
            }
        }
        
        if (ScriptsList.Length >= 1024 || MissingDependsList.Length >= 1024)
        {
            await eventArgs.Interaction.DeferAsync(true);
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many Scripts to display in a single message.", true);
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Script:**",
                Description = ScriptsList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Missing Depend:**",
                Description = MissingDependsList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (ScriptsList.Length != 0) overflow.AddEmbed(embed2);
            if (MissingDependsList.Length != 0) overflow.AddEmbed(embed3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ShvdnDetailedSendToUser, "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            DiscordMessage? sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflow);
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
            return;
        }
        
        if (log.Scripts.Count > 0)
            embed.AddField(":orange_circle:     Script:", ScriptsList, true);
        
        if (log.MissingDepends.Count > 0) 
            embed.AddField(":red_circle:     Missing Depend:", MissingDependsList, true);
            
        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.ShvdnDetailedSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
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