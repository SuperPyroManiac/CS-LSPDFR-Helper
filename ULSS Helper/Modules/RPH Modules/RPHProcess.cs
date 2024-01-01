using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;

// ReSharper disable InconsistentNaming
internal class RPHProcess : SharedLogInfo
{
    internal string current;
    internal string outdated;
    internal string broken;
    internal string missing;
    internal string library;
    internal string missmatch;
    internal string rph;
    internal RPHLog log;
    internal string GtAver = "❌";
    internal string LspdfRver = "❌";
    internal string RpHver = "❌";

    private DiscordEmbedBuilder GetBaseLogInfoEmbed(string description)
    {
        if (Program.Settings.Env.GtaVersion.Equals(log.GTAVersion)) GtAver = "\u2713";
        if (Program.Settings.Env.LspdfrVersion.Equals(log.LSPDFRVersion)) LspdfRver = "\u2713";
        if (Program.Settings.Env.RphVersion.Equals(log.RPHVersion)) RpHver = "\u2713";

        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"GTA: {GtAver} - RPH: {RpHver}" +
                       $" - LSPDFR: {LspdfRver} - Notes: {log.Errors.Count}"
            }
        };
    }

    internal DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed)
    {
        if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
        if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";

        if (outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + outdated, true);
        if (broken.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + broken, true);
        if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing);
        if (missmatch.Length > 0) embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatch);

        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && !string.IsNullOrEmpty(log.LSPDFRVersion))
            embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
            embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
            embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
        
        if (current.Length == 0) current = "**None**";
        if (rph.Length == 0) rph = "**None**";

        return embed;
    }

    internal async Task SendQuickLogInfoMessage(ContextMenuContext context=null, ComponentInteractionCreateEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        
        var linkedOutdated = log.Outdated.Select(
            plugin => plugin.Link != null && plugin.Link.StartsWith("https://")
                ? $"[{plugin.DName}]({plugin.Link})"
                : $"[{plugin.DName}](https://www.google.com/search?q=lspdfr+{plugin.DName.Replace(" ", "+")})"
        ).ToList();
        
        var _currentList = log.Current.Select(plugin => plugin?.DName).ToList();
        var brokenList = log.Broken.Select(plugin => plugin?.DName).ToList();
        var missingList = log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList();
        var missmatchList = log.Missmatch.Select(plugin => $"{plugin?.Name} ({plugin?.EAVersion})").ToList();
        var libraryList = log.Library.Select(plugin => plugin?.DName).ToList();
        var rphList = log.RPHPlugin.Select(plugin => plugin?.Name).ToList();
        brokenList.AddRange(libraryList);
        current = string.Join(", ", _currentList);
        outdated = string.Join("\r\n- ", linkedOutdated);
        broken = string.Join("\r\n- ", brokenList);
        missing = string.Join(", ", missingList);
        missmatch = string.Join(", ", missmatchList);
        library = string.Join(", ", libraryList);
        rph = string.Join(", ", rphList);
        
        var embedDescription = "## RPH.log Quick Info";        
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";

        var embed = GetBaseLogInfoEmbed(embedDescription);

        var targetMessage = context?.TargetMessage ?? eventArgs.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, log);


        if (missmatch.Length > 0 || missing.Length > 0) SendUnknownPluginsLog(cache.OriginalMessage.Channel.Id, cache.OriginalMessage.Author.Id);
        
        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n>>> " + string.Join(" - ", linkedOutdated),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n>>> " + string.Join(" - ", brokenList),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflowBuilder = new DiscordWebhookBuilder();
            overflowBuilder.AddEmbed(embed);
            if (outdated.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (broken.Length != 0) overflowBuilder.AddEmbed(embed3);
            // ReSharper disable RedundantExplicitParamsArrayCreation
            overflowBuilder.AddComponents([
                new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RphQuickSendToUser, "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            ]);
            
            DiscordMessage sentOverflowMessage;
            if (context != null)
                sentOverflowMessage = await context.EditResponseAsync(overflowBuilder);
            else if (eventArgs.Id == ComponentInteraction.RphGetQuickInfo)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder(overflowBuilder);
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                sentOverflowMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
            }
            else
                sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflowBuilder);
                 
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new(cache.Interaction, cache.OriginalMessage, this));
        }
        else
        {
            embed = AddCommonFields(embed);
            
            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embed);
            webhookBuilder.AddComponents(
                [
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.RphGetDetailedInfo, "Error Info", false, new DiscordComponentEmoji("❗")),
                    new DiscordButtonComponent(ButtonStyle.Primary, ComponentInteraction.RphGetAdvancedInfo, "Plugin Info", false, new DiscordComponentEmoji("❓")),
                    new DiscordButtonComponent(ButtonStyle.Danger, ComponentInteraction.RphQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                ]
            );

            DiscordMessage sentMessage;
            if (context != null)
                sentMessage = await context.EditResponseAsync(webhookBuilder);
            else if (eventArgs.Id == ComponentInteraction.RphGetQuickInfo)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder(webhookBuilder);
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
            }
            else
                sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
                
            Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
        }
    }

    internal async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        var embedDescription = "## RPH.log Error Info";
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";
        var embed = GetBaseLogInfoEmbed(embedDescription);

        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache, log);
        
        embed = AddCommonFields(embed);

        var ts = Database.LoadUsers().FirstOrDefault(ts => ts.UID.ToString().Equals(eventArgs.User.Id.ToString()));
        var errorIds = new List<DiscordSelectComponentOption>();
        var update = false;
        foreach (var error in log.Errors)
        {
            if (embed.Fields.Count == 24)
            {
                embed.AddField("**TOO MANY ERRORS**", "God Damn! Fix some errors and try again!\r\nCannot show more than 25 fields per message.");
                break;
            }
            
            if (error.Level == "CRITICAL") update = true;
            if (update)
            {
                if (error.Level == "CRITICAL")
                    embed.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
            }
            if (!update)
            {
                if (ts == null || ts.View == 1)
                    embed.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
            
                // ReSharper disable MergeIntoPattern
                if (ts != null && ts.View == 0 && error.Level != "XTRA")
                    embed.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
            }
            if (error.Level != "XTRA")
            {
                if (errorIds.All(x => x.Value != error.ID)) errorIds.Add(new DiscordSelectComponentOption("ID: " + error.ID, error.ID));;
            }
        }

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);
        if (errorIds.Count > 0 && !update) 
            responseBuilder.AddComponents(
                new DiscordSelectComponent(
                    customId: ComponentInteraction.SelectIdForRemoval, 
                    placeholder: "Remove Error", 
                    options: errorIds
                )
            );

        responseBuilder.AddComponents(
            [
                new DiscordButtonComponent(
                    ButtonStyle.Secondary,
                    ComponentInteraction.RphGetQuickInfo,
                    "Back to Quick Info", 
                    false,
                    new DiscordComponentEmoji("⬅️")
                ),
                new DiscordButtonComponent(
                    ButtonStyle.Danger,
                    ComponentInteraction.RphDetailedSendToUser,
                    "Send To User", 
                    false,
                    new DiscordComponentEmoji("📨")
                )
            ]
        );

        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
    }
    
    internal async Task SendAdvancedInfoMessage(ComponentInteractionCreateEventArgs eventArgs)
    {
        var embedDescription = "## RPH.log Plugin Info";
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";
        var embed = GetBaseLogInfoEmbed(embedDescription);

        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache, log);
        
        embed = AddCommonFields(embed);
        if (current.Length >= 1024) current = "Too many plugins to show!";
        if (rph.Length >= 1024) current = "Too many plugins to show!";
        
        embed.AddField(":jigsaw:     **Up To Date:**", "\r\n>>> - " + current, false);
        embed.AddField(":purple_circle:     **RPH Plugins:**", "\r\n>>> - " + rph, false);

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);

        responseBuilder.AddComponents(
            [
                new DiscordButtonComponent(
                    ButtonStyle.Secondary,
                    ComponentInteraction.RphGetQuickInfo,
                    "Back to Quick Info", 
                    false,
                    new DiscordComponentEmoji("⬅️")
                )
            ]
        );

        await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new(cache.Interaction, cache.OriginalMessage, this)); 
    }
    
    internal async Task SendMessageToUser(ComponentInteractionCreateEventArgs eventArgs)
    {
        var newEmbList = new List<DiscordEmbed>();
        var embedDescription = eventArgs.Message.Embeds[0].Description;
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
        var embedDescription = "__Unknown plugins or plugin versions!__\r\n\r\n>>> ";
        var rphLogLink = log.DownloadLink != null && log.DownloadLink.StartsWith("http")
            ? $"[RagePluginHook.log]({log.DownloadLink})" 
            : "RagePluginHook.log";
        embedDescription += $"There was a {rphLogLink} uploaded that has plugin versions that are unknown to the bot's DB!\r\n\r\n";
        var embed = BasicEmbeds.Warning(embedDescription, true);

        var missingList = log.Missing.Select(plugin => $"{plugin?.Name} {plugin?.Version}").ToList();
        var missmatchList = log.Missmatch.Select(plugin => $"{plugin?.Name} {plugin?.EAVersion}").ToList();
        
        var missingDashListStr = "- " + string.Join("\n- ", missingList) + "\n";
        if (missingDashListStr.Length > 3 && missingDashListStr.Length < 1024)
        {
            embed.AddField(":bangbang:  **Plugins not recognized:**", missingDashListStr);
        }

        var missmatchDashListStr = "- " + string.Join("\n- ", missmatchList) + "\n";
        if (missmatchDashListStr.Length > 3 && missmatchDashListStr.Length < 1024)
        {
            embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatchDashListStr);
        }

        if (missingDashListStr.Length >= 1024 || missmatchDashListStr.Length >= 1024)
            embed.AddField("Attention!", "Too many unknown plugins to display them in this message. Please check the log manually.");

        Logging.SendLog(originalMsgChannelId, originalMsgUserId, embed);
    }
}