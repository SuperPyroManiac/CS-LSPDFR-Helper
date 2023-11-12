using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;
internal class RPHProcess : SharedLogInfo
{
    internal string current;
    private List<string?> currentList;
    internal string outdated;
    internal string broken;
    internal string missing;
    internal string library;
    internal string missmatch;
    internal RPHLog log;
    internal string GTAver = "X";
    internal string LSPDFRver = "X";
    internal string RPHver = "X";

    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description)
    {
        if (Settings.GTAVer.Equals(log.GTAVersion)) GTAver = "\u2713";
        if (Settings.LSPDFRVer.Equals(log.LSPDFRVersion)) LSPDFRver = "\u2713";
        if (Settings.RPHVer.Equals(log.RPHVersion)) RPHver = "\u2713";

        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"GTA: {GTAver} - RPH: {RPHver}" +
                       $" - LSPDFR: {LSPDFRver} - Notes: {log.Errors.Count}"
            }
        };
    }

    internal DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed)
    {
        if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
        if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";
        if ((current.Length == 0 && outdated.Length != 0) || broken.Length != 0) current = "**None**";

        if (outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
        if (broken.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);
        if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing);
        if (missmatch.Length > 0) embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatch);

        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0)
            embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
            embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
            embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");

        return embed;
    }

    internal async Task SendQuickLogInfoMessage(ContextMenuContext? context=null, ComponentInteractionCreateEventArgs? eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        
        var linkedOutdated = log.Outdated.Select(i => i?.Link != null
                ? $"[{i.DName}]({i.Link})"
                : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})")
            .ToList();
        
        currentList = log.Current.Select(i => i?.DName).ToList();
        var brokenList = log.Broken.Select(i => i?.DName).ToList();
        var missingList = log.Missing.Select(i => i?.Name).ToList();
        var missmatchList = log.Missmatch.Select(i => i?.Name).ToList();
        var libraryList = log.Library.Select(i => i?.DName).ToList();
        brokenList.AddRange(libraryList);
        current = string.Join("\r\n- ", currentList);
        outdated = string.Join("\r\n- ", linkedOutdated);
        broken = string.Join("\r\n- ", brokenList);
        missing = string.Join(", ", missingList);
        missmatch = string.Join(", ", missmatchList);
        library = string.Join(", ", libraryList);
        
        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("## Quick RPH.log Info");

        DiscordMessage targetMessage = context?.TargetMessage ?? eventArgs.Message;
        ProcessCache cache = Program.Cache.GetProcessCache(targetMessage.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage);
        
        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n- " + outdated,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n- " + broken,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (outdated.Length != 0) overflow.AddEmbed(embed2);
            if (broken.Length != 0) overflow.AddEmbed(embed3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RphQuickSendToUser, "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            
            DiscordMessage? sentOverflowMessage;
            if (context != null)
                sentOverflowMessage = await context.EditResponseAsync(overflow);
            else
                sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflow);
                 
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this));
        }
        else
        {
            embed = AddCommonFields(embed);
            
            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embed);
            webhookBuilder.AddComponents(
                new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.RphGetDetailedInfo, "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RphQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                }
            );

            DiscordMessage? sentMessage;
            if (context != null)
                sentMessage = await context.EditResponseAsync(webhookBuilder);
            else
                sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
                
            Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
         }
    }

    internal async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        var embed = GetBaseLogInfoEmbed("## Detailed RPH.log Info");
        ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage);
        
        embed = AddCommonFields(embed);

        foreach (var error in log.Errors)
        {
            embed.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:", $"> {error.Solution.Replace("\n", "\n> ")}");
        }

        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RphDetailedSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
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
            if (!field.Name.Contains(":bangbang:")) newEmb.AddField(field.Name, field.Value, field.Inline);
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