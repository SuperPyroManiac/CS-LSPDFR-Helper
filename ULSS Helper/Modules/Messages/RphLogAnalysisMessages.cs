using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Messages;
internal static class RphLogAnalysisMessages 
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless";
    internal static string current;
    private static List<string?> currentList;
    internal static string outdated;
    internal static string broken;
    internal static string missing;
    internal static string library;
    internal static string missmatch;
    private static ulong senderID;
    internal static AnalyzedRphLog log;
    internal static string GTAver = "X";
    internal static string LSPDFRver = "X";
    internal static string RPHver = "X";

    internal static DiscordEmbedBuilder GetBaseLogInfoMessage(string description)
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

    internal static DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder message)
    {
        if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
        if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";
        if ((current.Length == 0 && outdated.Length != 0) || broken.Length != 0) current = "**None**";

        if (outdated.Length > 0) message.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
        if (broken.Length > 0) message.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);
        if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing);
        if (missmatch.Length > 0) message.AddField(":bangbang:  **Plugin version newer than DB:**", missmatch);

        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0)
            message.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
            message.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
            message.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");

        return message;
    }

    internal static async Task SendQuickLogInfoMessage(ContextMenuContext e)
    {
        var linkedOutdated = log.Outdated.Select(i => i?.Link != null
                ? $"[{i.DName}]({i.Link})"
                : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})")
            .ToList();
        senderID = e.TargetMessage.Author.Id;
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
        
        var message = GetBaseLogInfoMessage("# **Quick Log Information**");

        message.AddField("Log uploader:", $"{e.TargetMessage.Author.Mention}", true);
        message.AddField("Log message:", e.TargetMessage.JumpLink.ToString(), true);
        message.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        
        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            message.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
            var message2 = new DiscordEmbedBuilder { Title = ":orange_circle:     **Update:**", Description = "\r\n- " + outdated, Color = DiscordColor.Gold };
            message2.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon };
            var message3 = new DiscordEmbedBuilder { Title = ":red_circle:     **Remove:**", Description = "\r\n- " + broken, Color = DiscordColor.Gold };
            message3.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon };
            
            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(message);
            if (outdated.Length != 0) overflow.AddEmbed(message2);
            if (broken.Length != 0) overflow.AddEmbed(message3);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            await e.EditResponseAsync(overflow);
        }
        else
        {
            message = AddCommonFields(message);
            
            await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false, new DiscordComponentEmoji("📨"))
            }));
        }
    }

    internal static async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        
        var message = GetBaseLogInfoMessage("# **Detailed Log Information**");
        
        message.AddField("Log uploader:", $"<@{senderID}>", true);
        message.AddField("Log message:", e.Message.JumpLink.ToString(), true);
        message.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        
        message = AddCommonFields(message);

        foreach (var error in log.Errors)
        {
            message.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:", $"> {error.Solution}");
        }
            
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, "send2", "Send To User", false, new DiscordComponentEmoji("📨"))
        }));
    }
    
    internal static async Task SendMessageToUser(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseLogInfoMessage(e.Message.Embeds[0].Description);
        
        foreach (var field in e.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains(":bangbang:")) newEmb.AddField(field.Name, field.Value, field.Inline);
        }
        newEmb.RemoveFieldRange(0, 3);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(e.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.WithContent($"<@{senderID}>");
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithAllowedMention(new UserMention(senderID));
        await e.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(e.Channel);
    }
}