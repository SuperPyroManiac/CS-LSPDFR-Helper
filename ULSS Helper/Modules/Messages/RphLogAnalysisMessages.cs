using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Messages;
internal class RphLogAnalysisMessages : LogAnalysisMessages
{
    internal static string current;
    private static List<string?> currentList;
    internal static string outdated;
    internal static string broken;
    internal static string missing;
    internal static string library;
    internal static string missmatch;
    internal static AnalyzedRphLog log;
    internal static string GTAver = "X";
    internal static string LSPDFRver = "X";
    internal static string RPHver = "X";

    private static DiscordEmbedBuilder GetBaseLogInfoEmbed(string description)
    {
        if (Settings.GTAVer.Equals(log.GTAVersion)) GTAver = "\u2713";
        if (Settings.LSPDFRVer.Equals(log.LSPDFRVersion)) LSPDFRver = "\u2713";
        if (Settings.RPHVer.Equals(log.RPHVersion)) RPHver = "\u2713";

        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = DiscordColor.Gold,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"GTA: {GTAver} - RPH: {RPHver}" +
                       $" - LSPDFR: {LSPDFRver} - Notes: {log.Errors.Count}"
            }
        };
    }

    internal static DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed)
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

    internal static async Task SendQuickLogInfoMessage(ContextMenuContext e)
    {
        var linkedOutdated = log.Outdated.Select(i => i?.Link != null
                ? $"[{i.DName}]({i.Link})"
                : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})")
            .ToList();
        logUploaderUserId = e.TargetMessage.Author.Id;
        logMessageLink = e.TargetMessage.JumpLink.ToString();
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

        embed = AddTsViewFields(embed);
        
        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n- " + outdated,
                Color = DiscordColor.Gold,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n- " + broken,
                Color = DiscordColor.Gold,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (outdated.Length != 0) overflow.AddEmbed(embed2);
            if (broken.Length != 0) overflow.AddEmbed(embed3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            await e.EditResponseAsync(overflow);
        }
        else
        {
            embed = AddCommonFields(embed);
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false, new DiscordComponentEmoji("📨"))
            }));
        }
    }

    internal static async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs e)
    {
        var embed = GetBaseLogInfoEmbed("## Detailed RPH.log Info");
        
        embed = AddTsViewFields(embed);
        
        embed = AddCommonFields(embed);

        foreach (var error in log.Errors)
        {
            embed.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:", $"> {error.Solution.Replace("\n", "\n> ")}");
        }

        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "send2", "Send To User", false, new DiscordComponentEmoji("📨"))
            }));
    }
    
    internal static async Task SendMessageToUser(ComponentInteractionCreateEventArgs e)
    {
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseLogInfoEmbed(e.Message.Embeds[0].Description);
        
        foreach (var field in e.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains(":bangbang:")) newEmb.AddField(field.Name, field.Value, field.Inline);
        }
        
        newEmb = RemoveTsViewFields(newEmb);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(e.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithReply(log.MsgId, true);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Sent!")));
        await e.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(e.Channel);
    }
}