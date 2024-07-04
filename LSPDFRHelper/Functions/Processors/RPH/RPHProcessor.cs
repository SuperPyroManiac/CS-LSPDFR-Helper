using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;
using ProcessCache = LSPDFRHelper.CustomTypes.CacheTypes.ProcessCache;
using RPHLog = LSPDFRHelper.CustomTypes.LogTypes.RPHLog;
using State = LSPDFRHelper.CustomTypes.Enums.State;

namespace LSPDFRHelper.Functions.Processors.RPH;

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

    private void InitValues()
    {
        _current = string.Join(", ", Log.Current.Select(plugin => plugin?.DName).ToList());
        _remove = string.Join("\r\n- ", ( from plug in Log.Current where (plug.State == State.BROKEN || plug.PluginType == PluginType.LIBRARY) select plug.DName ).ToList());
        _missing = string.Join(", ", Log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList());
        _missmatch = string.Join(", ", Log.NewVersion.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList());
        _rph = string.Join(", ", ( from plug in Log.Current where plug.PluginType == PluginType.RPH select plug.DName ).ToList());
        _outdated = string.Join("\r\n- ",
            Log.Outdated.Select(plugin => plugin.Link != null && plugin.Link.StartsWith("https://")
                ? $"[{plugin.DName}]({plugin.Link})"
                : $"[{plugin.DName}](https://www.google.com/search?q=lspdfr+{plugin.DName.Replace(" ", "+")})").ToList());
    }
    
    private DiscordEmbedBuilder GetBaseEmbed(string description)
    {
        if (Program.Cache.GetPlugin("GrandTheftAuto5").Version.Equals(Log.GTAVersion)) _gtaVer = "\u2713";
        if (Program.Cache.GetPlugin("LSPDFR").Version.Equals(Log.LSPDFRVersion)) _lspdfrVer = "\u2713";
        if (Program.Cache.GetPlugin("RagePluginHook").Version.Equals(Log.RPHVersion)) _rphVer = "\u2713";
        return BasicEmbeds.Ts(description + BasicEmbeds.AddBlanks(20),
            new DiscordEmbedBuilder.EmbedFooter { Text = $"GTA: {_gtaVer} - RPH: {_lspdfrVer} - LSPDFR: {_rphVer} - Notes: {Log.Errors.Count} - Try /CheckPlugin" });
    }

    private DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed, bool ts = true)
    {
        if (_current.Length != 0 && _outdated.Length == 0 && _remove.Length != 0) _outdated = "**None**";
        if (_current.Length != 0 && _outdated.Length != 0 && _remove.Length == 0) _remove = "**None**";

        if (_outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + _outdated, true);
        if (_remove.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + _remove, true);
        if (_missing.Length > 0 && ts) embed.AddField(":bangbang:  **Plugins not recognized:**", _missing);
        if (_missmatch.Length > 0 && ts) embed.AddField(":bangbang:  **Plugin version newer than DB:**", _missmatch);

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
    
    public async Task SendQuickInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null) throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        if (targetMessage == null) targetMessage = eventArgs!.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        
        InitValues();
        if (_missmatch.Length > 0 || _missing.Length > 0) await SendUnknownPluginsLog(cache.OriginalMessage.Channel!.Id, cache.OriginalMessage.Author!.Id, Log.DownloadLink, Log.Missing, Log.NewVersion);

        var embed = GetBaseEmbed("## __RPH.log Quick Info__");
        embed = AddTsViewFields(embed, cache, Log);
        if (_outdated.Length >= 1024 || _remove.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (_missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", _missing);
            var embed2 = BasicEmbeds.Public("\r\n>>> " + string.Join(" - ", _outdated));
            var embed3 = BasicEmbeds.Public("\r\n>>> " + string.Join(" - ", _remove));
            
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
        var update = Log.Errors.Any(x => x.Level == Level.CRITICAL);
        foreach (var error in Log.Errors)
        {
            if ( error.Solution.Length >= 1023 ) error.Solution = "ERROR: The solutions text was too big to display here! Please report this to SuperPyroManiac.";
            if (embed.Fields.Count == 20)
            {
                embed.AddField("___```Too Much``` Overflow:___",
                    ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                break;
            }

            if (error.Level == Level.CRITICAL)
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
            responseBuilder.AddComponents(new DiscordSelectComponent(customId: CustomIds.SelectIdForRemoval, placeholder: "Remove Error", options: errorIds));
        responseBuilder.AddComponents(
            [
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.RphGetQuickInfo, "Back to Quick Info", false, new DiscordComponentEmoji("⬅️")), 
                new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
            ]);

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
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.RphGetQuickInfo, "Back to Quick Info", false, new DiscordComponentEmoji("⬅️")),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
            ]);

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
    
    public async Task SendAutoHelperMessage(MessageCreatedEventArgs ctx)
    {
        InitValues();
        if (_missmatch.Length > 0 || _missing.Length > 0) await SendUnknownPluginsLog(ctx.Message.Channel!.Id, ctx.Message.Author!.Id, Log.DownloadLink, Log.Missing, Log.NewVersion);

        
        var embed = GetBaseEmbed("## __AutoHelper RPH.log Info__");
        if (_outdated.Length >= 1024 || _remove.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor error info, first fix the plugins!", true);
            var embed2 = BasicEmbeds.Public("\r\n>>> " + string.Join(" - ", _outdated));
            var embed3 = BasicEmbeds.Public("\r\n>>> " + string.Join(" - ", _remove));

            var overflowBuilder = new DiscordMessageBuilder();
            overflowBuilder.AddEmbed(embed);
            if (_outdated.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (_remove.Length != 0) overflowBuilder.AddEmbed(embed3);
            await ctx.Message.RespondAsync(overflowBuilder);
            return;
        }
        embed = AddCommonFields(embed, false);
        
        var update = Log.Errors.Any(x => x.Level == Level.CRITICAL);
        var cnt = 0;
        foreach (var error in Log.Errors.Where(x => x.Level != Level.XTRA))
        {
            if ( error.Solution.Length >= 1023 ) error.Solution = "ERROR: The solutions text was too big to display here! Please report this to SuperPyroManiac.";
            if (embed.Fields.Count == 20)
            {
                embed.AddField("___```Too Much``` Overflow:___",
                    ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                break;
            }
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
            cnt++;
        }
        
        

        var responseBuilder = new DiscordMessageBuilder();
        responseBuilder.AddEmbed(embed);
        if ( _outdated.Length == 0 && _remove.Length == 0 && cnt == 0 ) responseBuilder.AddEmbed(BasicEmbeds.Success("__No Issues Detected__\r\n>>> If you do have any problems, you may want to post in the public support channels!"));//TODO: Better gen message
        await ctx.Message.RespondAsync(responseBuilder);
    }
}