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
        if (Program.Settings.Env.GtaVersion.Equals(log.GTAVersion)) GTAver = "\u2713";
        if (Program.Settings.Env.LspdfrVersion.Equals(log.LSPDFRVersion)) LSPDFRver = "\u2713";
        if (Program.Settings.Env.RphVersion.Equals(log.RPHVersion)) RPHver = "\u2713";

        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
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
        
        var linkedOutdated = log.Outdated.Select(
            plugin => (plugin?.Link != null && plugin.Link.StartsWith("https://"))
                ? $"[{plugin.DName}]({plugin.Link})"
                : $"[{plugin?.DName}](https://www.google.com/search?q=lspdfr+{plugin.DName.Replace(" ", "+")})"
        ).ToList();
        
        currentList = log.Current.Select(plugin => plugin?.DName).ToList();
        var brokenList = log.Broken.Select(plugin => plugin?.DName).ToList();
        var missingList = log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList();
        var missmatchList = log.Missmatch.Select(plugin => $"{plugin?.Name} ({plugin?.EAVersion})").ToList();
        var libraryList = log.Library.Select(plugin => plugin?.DName).ToList();
        brokenList.AddRange(libraryList);
        current = string.Join("\r\n- ", currentList);
        outdated = string.Join("\r\n- ", linkedOutdated);
        broken = string.Join("\r\n- ", brokenList);
        missing = string.Join(", ", missingList);
        missmatch = string.Join(", ", missmatchList);
        library = string.Join(", ", libraryList);
        
        string embedDescription = "## Quick RPH.log Info";        
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";

        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed(embedDescription);

        DiscordMessage targetMessage = context?.TargetMessage ?? eventArgs.Message;
        ProcessCache cache = Program.Cache.GetProcessCache(targetMessage.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage, log.ElapsedTime);


        if (missmatch.Length > 0 || missing.Length > 0) SendUnknownPluginsLog(cache.OriginalMessage.Channel.Id, cache.OriginalMessage.Author.Id);
        
        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n- " + outdated,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n- " + broken,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
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
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.RphGetDetailedInfo, "More Info", false, new DiscordComponentEmoji(Program.Settings.Env.MoreInfoBtnEmojiId)),
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
        string embedDescription = "## Detailed RPH.log Info";
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";
        var embed = GetBaseLogInfoEmbed(embedDescription);

        ProcessCache cache = Program.Cache.GetProcessCache(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache.OriginalMessage, log.ElapsedTime);
        
        embed = AddCommonFields(embed);

        var ts = Database.LoadTS().FirstOrDefault(x => x.ID.ToString() == eventArgs.User.Id.ToString());
        var update = false;
        foreach (var error in log.Errors)
        {
            if (error.Level == "CRITICAL") update = true;
            if (update)
            {
                if (error.Level == "CRITICAL")
                    embed.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:", $"> {error.Solution.Replace("\n", "\n> ")}");
            }
            if (!update)
            {
                if (ts.View == 1)
                    embed.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
            
                if (ts.View == 0 && error.Level != "XTRA")
                    embed.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
            }
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
        string embedDescription = eventArgs.Message.Embeds[0].Description;
        if (outdated.Length > 0 || broken.Length > 0) 
            embedDescription += "\r\n\r\nUpdate or remove the following files in `GTAV/plugins/LSPDFR`";
        var newEmb = GetBaseLogInfoEmbed(embedDescription);
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains(":bangbang:") && !field.Name.Contains("XTRA")) newEmb.AddField(field.Name, field.Value, field.Inline);
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

    internal void SendUnknownPluginsLog(ulong originalMsgChannelId, ulong originalMsgUserId)
    {
        string embedDescription = "**Unknown plugins or plugin versions!**\r\n\r\n";
        string rphLogLink = (log.DownloadLink != null && log.DownloadLink.StartsWith("http")) 
            ? $"[RagePluginHook.log]({log.DownloadLink})" 
            : "RagePluginHook.log";
        embedDescription += $"There was a {rphLogLink} uploaded that has plugins or plugin versions that are unknown to the bot's DB!\r\n\r\n";
        DiscordEmbedBuilder embed = BasicEmbeds.Warning(embedDescription);

        var missingList = log.Missing.Select(plugin => $"{plugin?.Name} {plugin?.Version}").ToList();
        var missmatchList = log.Missmatch.Select(plugin => $"{plugin?.Name} {plugin?.EAVersion}").ToList();
        
        string missingDashListStr = "- " + string.Join("\n- ", missingList) + "\n";
        if (missingDashListStr.Length > 0 && missingDashListStr.Length < 1024) 
        {
            embed.AddField(":bangbang:  **Plugins not recognized:**", missingDashListStr);
        }

        string missmatchDashListStr = "- " + string.Join("\n- ", missmatchList) + "\n";
        if (missmatchDashListStr.Length > 0 && missmatchDashListStr.Length < 1024) 
        {
            embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatchDashListStr);
        }

        if (missingDashListStr.Length >= 1024 || missmatchDashListStr.Length >= 1024)
            embed.AddField("Attention!", "Too many unknown plugins to display them in this message. Please check the log manually.");

        Logging.SendLog(originalMsgChannelId, originalMsgUserId, embed);
    }
}