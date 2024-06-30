using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions.Messages;
using ProcessCache = LSPDFR_Helper.CustomTypes.CacheTypes.ProcessCache;
using RPHLog = LSPDFR_Helper.CustomTypes.LogTypes.RPHLog;
using State = LSPDFR_Helper.CustomTypes.Enums.State;

namespace LSPDFR_Helper.Functions.Processors.RPH;

public class RphProcessor : SharedData
{
    public RPHLog Log;
    private string _current;
    private string _outdated;
    private string _remove;
    private string _missing;
    private string _missmatch;
    private string _rph;
    private string _gtaVer = "❌";
    private string _lspdfrVer = "❌";
    private string _rphVer = "❌";
    
    private DiscordEmbedBuilder GetBaseEmbed(string description)
    {
        if (Program.Cache.GetPlugin("GrandTheftAuto5").Version.Equals(Log.GTAVersion)) _gtaVer = "\u2713";
        if (Program.Cache.GetPlugin("LSPDFR").Version.Equals(Log.LSPDFRVersion)) _lspdfrVer = "\u2713";
        if (Program.Cache.GetPlugin("RagePluginHook").Version.Equals(Log.RPHVersion)) _rphVer = "\u2713";
        return BasicEmbeds.Ts(description + BasicEmbeds.AddBlanks(20),
            new DiscordEmbedBuilder.EmbedFooter { Text = $"GTA: {_gtaVer} - RPH: {_lspdfrVer} - LSPDFR: {_rphVer} - Notes: {Log.Errors.Count} - Try /CheckPlugin" });
    }
    
    private DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed)
    {
        if (_current.Length != 0 && _outdated.Length == 0 && _remove.Length != 0) _outdated = "**None**";
        if (_current.Length != 0 && _outdated.Length != 0 && _remove.Length == 0) _remove = "**None**";

        if (_outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + _outdated, true);
        if (_remove.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + _remove, true);
        if (_missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", _missing);
        if (_missmatch.Length > 0) embed.AddField(":bangbang:  **Plugin version newer than DB:**", _missmatch);

        if (_current.Length > 0 && _outdated.Length == 0 && _remove.Length == 0 && !string.IsNullOrEmpty(Log.LSPDFRVersion))
            embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (_current.Length > 0 && _outdated.Length == 0 && _remove.Length == 0 && string.IsNullOrEmpty(Log.LSPDFRVersion))
            embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (_current.Length == 0 && _outdated.Length == 0 && _remove.Length == 0)
            embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
        
        if (_current.Length == 0) _current = "**None**";
        if (_rph.Length == 0) _rph = "**None**";

        return embed;
    }
    
    public async Task SendUnknownPluginsLog(ulong originalMsgChannelId, ulong originalMsgUserId)
    {
        var rphLogLink = Log.DownloadLink != null && Log.DownloadLink.StartsWith("http")
            ? $"[RagePluginHook.log]({Log.DownloadLink})" 
            : "RagePluginHook.log";
        var embed = BasicEmbeds.Warning($"__Unknown Plugin / Version!__{BasicEmbeds.AddBlanks(35)}\r\n\r\n>>> " +
                                        $"There was a {rphLogLink} uploaded that has plugin versions that are unknown to the bot's DB!\r\n\r\n");
        
        var missingDashListStr = "- " + string.Join("\n- ", Log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList()) + "\n";
        if (missingDashListStr.Length is > 3 and < 1024) embed.AddField(":bangbang:  **Plugins not recognized:**", missingDashListStr);

        var missmatchDashListStr = "- " + string.Join("\n- ", Log.NewVersion.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList()) + "\n";
        if (missmatchDashListStr.Length is > 3 and < 1024) embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatchDashListStr);

        if (missingDashListStr.Length >= 1024 || missmatchDashListStr.Length >= 1024)
            embed.AddField("Attention!", "Too many unknown plugins to display them in this message. Please check the log manually.");//TODO: This is most likely abuse!

        await Logging.SendLog(originalMsgChannelId, originalMsgUserId, embed);
    }
    
    public async Task SendQuickInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        
        _outdated = string.Join("\r\n- ",
            Log.Outdated.Select(plugin => plugin.Link != null && plugin.Link.StartsWith("https://")
                ? $"[{plugin.DName}]({plugin.Link})"
                : $"[{plugin.DName}](https://www.google.com/search?q=lspdfr+{plugin.DName.Replace(" ", "+")})").ToList());
        
        _current = string.Join(", ", Log.Current.Select(plugin => plugin?.DName).ToList());
        _remove = string.Join("\r\n- ", ( from plug in Log.Current where (plug.State == State.BROKEN || plug.PluginType == PluginType.LIBRARY) select plug.DName ).ToList());
        _missing = string.Join(", ", Log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList());
        _missmatch = string.Join(", ", Log.NewVersion.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList());
        _rph = string.Join(", ", ( from plug in Log.Current where plug.PluginType == PluginType.RPH select plug.DName ).ToList());
        
        var embed = GetBaseEmbed("## __RPH.log Quick Info__");
        if (targetMessage == null) targetMessage = eventArgs!.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, Log);


        if (_missmatch.Length > 0 || _missing.Length > 0) await SendUnknownPluginsLog(cache.OriginalMessage.Channel!.Id, cache.OriginalMessage.Author!.Id);
        
        if (_outdated.Length >= 1024 || _remove.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (_missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", _missing);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n>>> " + string.Join(" - ", _outdated),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n>>> " + string.Join(" - ", _remove),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl }
            };

            var overflowBuilder = new DiscordWebhookBuilder();
            overflowBuilder.AddEmbed(embed);
            if (_outdated.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (_remove.Length != 0) overflowBuilder.AddEmbed(embed3);
            overflowBuilder.AddComponents([new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))]);
            
            DiscordMessage sentOverflowMessage;
            if (context != null) sentOverflowMessage = await context.EditResponseAsync(overflowBuilder);
            else if (eventArgs.Id == CustomIds.RphGetQuickInfo)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder(overflowBuilder);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
                sentOverflowMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
            }
            else sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflowBuilder);
                 
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this));
        }
        else
        {
            embed = AddCommonFields(embed);
            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embed);
            webhookBuilder.AddComponents(
                [
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, CustomIds.RphGetErrorInfo, "Error Info", false, new DiscordComponentEmoji("❗")),
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, CustomIds.RphGetPluginInfo, "Plugin Info", false, new DiscordComponentEmoji("❓")),
                    new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
                ]
            );

            DiscordMessage sentMessage;
            if (context != null)
                sentMessage = await context.EditResponseAsync(webhookBuilder);
            else if (eventArgs.Id == CustomIds.RphGetQuickInfo)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder(webhookBuilder);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
                sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
            }
            else
                sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
                
            Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this)); 
        }
    }
    
    public async Task UpdateToErrorMessage(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var embed = GetBaseEmbed("## __RPH.log Error Info__");
        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache, Log);
        embed = AddCommonFields(embed);

        var errorIds = new List<DiscordSelectComponentOption>();
        var update = false;
        foreach (var error in Log.Errors)
        {
            if (embed.Fields.Count == 20)
            {
                embed.AddField("___```Too Much``` Overflow:___",
                    ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                break;
            }

            if (error.Level == Level.CRITICAL) update = true;
            switch (update)
            {
                case true:
                    if (error.Level == Level.CRITICAL)
                        embed.AddField($"___```{error.Level} ID: {error.Id}``` Troubleshooting Steps:___",
                            $"> {error.Solution.Replace("\n", "\n> ")}");
                    break;
                case false:
                    embed.AddField($"___```{error.Level} ID: {error.Id}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}");
                    break;
            }

            if (error.Level != Level.XTRA)
                if (errorIds.All(x => x.Value != error.Id.ToString())) errorIds.Add(new DiscordSelectComponentOption("Id: " + error.Id, error.Id.ToString()));;
            
        }

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);
        if (errorIds.Count > 0 && !update) 
            responseBuilder.AddComponents(
                new DiscordSelectComponent(
                    customId: CustomIds.SelectIdForRemoval, 
                    placeholder: "Remove Error", 
                    options: errorIds
                )
            );
        responseBuilder.AddComponents(
            [
                new DiscordButtonComponent(
                    DiscordButtonStyle.Secondary,
                    CustomIds.RphGetQuickInfo,
                    "Back to Quick Info", 
                    false,
                    new DiscordComponentEmoji("⬅️")
                ),
                new DiscordButtonComponent(
                    DiscordButtonStyle.Danger,
                    CustomIds.RphSendToUser,
                    "Send To User", 
                    false,
                    new DiscordComponentEmoji("📨")
                )
            ]
        );

        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this)); 
    }
    
    public async Task UpdateToPluginMessage(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var embed = GetBaseEmbed("## __RPH.log Plugin Info__");

        var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        embed = AddTsViewFields(embed, cache, Log);
        
        embed = AddCommonFields(embed);
        if (_current.Length >= 1024) _current = "Too many plugins to show!";
        if (_rph.Length >= 1024) _rph = "Too many plugins to show!";
        
        embed.AddField(":jigsaw:     **Up To Date:**", "\r\n>>> - " + _current);
        embed.AddField(":purple_circle:     **RPH Plugins:**", "\r\n>>> - " + _rph);

        var responseBuilder = new DiscordInteractionResponseBuilder();
        responseBuilder.AddEmbed(embed);

        responseBuilder.AddComponents(
            [
                new DiscordButtonComponent(
                    DiscordButtonStyle.Secondary,
                    CustomIds.RphGetQuickInfo,
                    "Back to Quick Info", 
                    false,
                    new DiscordComponentEmoji("⬅️")
                ),
                new DiscordButtonComponent(
                    DiscordButtonStyle.Danger,
                    CustomIds.RphSendToUser,
                    "Send To User", 
                    false,
                    new DiscordComponentEmoji("📨")
                )
            ]
        );

        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
        var sentMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this)); 
    }
    
    public async Task SendMessageToUser(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var newEmbList = new List<DiscordEmbed>();
        var embedDescription = eventArgs.Message.Embeds[0].Description;
        if (_outdated.Length > 0 || _remove.Length > 0) 
            embedDescription += "\r\n\r\nUpdate or remove the following files in `GTAV/plugins/LSPDFR`";
        var newEmb = GetBaseEmbed(embedDescription);
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields!)
        {
            if (!field.Name!.Contains(":bangbang:") && !field.Name.Contains("XTRA")) newEmb.AddField(field.Name, field.Value!, field.Inline);
        }
        
        newEmb = RemoveTsViewFields(newEmb);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(eventArgs.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithReply(Log.MsgId, true);
        newMessage.AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.SendFeedback,
            "Send Feedback", false, new DiscordComponentEmoji("📨")));
        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Sent!")));
        await eventArgs.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(eventArgs.Channel);
    }
    
}