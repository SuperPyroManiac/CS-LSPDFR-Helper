using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions.Messages;
using ULSS_Helper.Objects;
using ProcessCache = LSPDFR_Helper.CustomTypes.CacheTypes.ProcessCache;
using RPHLog = LSPDFR_Helper.CustomTypes.LogTypes.RPHLog;
using State = LSPDFR_Helper.CustomTypes.Enums.State;

namespace LSPDFR_Helper.Functions.Processors.RPH;

public class RphProcessor : SharedData
{
    public RPHLog Log;
    private string Current;
    private string Outdated;
    private string Remove;
    private string Missing;
    private string Missmatch;
    private string Rph;
    private string GtaVer = "❌";
    private string LspdfrVer = "❌";
    private string RphVer = "❌";
    
    private DiscordEmbedBuilder GetBaseEmbed(string description)
    {
        if (Program.Cache.GetPlugin("GrandTheftAuto5").Version.Equals(Log.GTAVersion)) GtaVer = "\u2713";
        if (Program.Cache.GetPlugin("LSPDFR").Version.Equals(Log.LSPDFRVersion)) LspdfrVer = "\u2713";
        if (Program.Cache.GetPlugin("RagePluginHook").Version.Equals(Log.RPHVersion)) RphVer = "\u2713";
        return BasicEmbeds.Ts(description + BasicEmbeds.AddBlanks(45),
            new DiscordEmbedBuilder.EmbedFooter() { Text = $"GTA: {GtaVer} - RPH: {LspdfrVer} - LSPDFR: {RphVer} - Notes: {Log.Errors.Count} - Try /CheckPlugin" });
    }
    
    private DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder embed)
    {
        if (Current.Length != 0 && Outdated.Length == 0 && Remove.Length != 0) Outdated = "**None**";
        if (Current.Length != 0 && Outdated.Length != 0 && Remove.Length == 0) Remove = "**None**";

        if (Outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + Outdated, true);
        if (Remove.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + Remove, true);
        if (Missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", Missing);
        if (Missmatch.Length > 0) embed.AddField(":bangbang:  **Plugin version newer than DB:**", Missmatch);

        if (Current.Length > 0 && Outdated.Length == 0 && Remove.Length == 0 && !string.IsNullOrEmpty(Log.LSPDFRVersion))
            embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (Current.Length > 0 && Outdated.Length == 0 && Remove.Length == 0 && string.IsNullOrEmpty(Log.LSPDFRVersion))
            embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (Current.Length == 0 && Outdated.Length == 0 && Remove.Length == 0)
            embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
        
        if (Current.Length == 0) Current = "**None**";
        if (Rph.Length == 0) Rph = "**None**";

        return embed;
    }
    
    public async Task SendUnknownPluginsLog(ulong originalMsgChannelId, ulong originalMsgUserId)
    {
        var rphLogLink = Log.DownloadLink != null && Log.DownloadLink.StartsWith("http")
            ? $"[RagePluginHook.log]({Log.DownloadLink})" 
            : "RagePluginHook.log";
        var embed = BasicEmbeds.Warning($"__Unknown Plugin / Version!__{BasicEmbeds.AddBlanks(35)}\r\n\r\n>>> " +
                                        $"There was a {rphLogLink} uploaded that has plugin versions that are unknown to the bot's DB!\r\n\r\n", true);
        
        var missingDashListStr = "- " + string.Join("\n- ", Log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList()) + "\n";
        if (missingDashListStr.Length is > 3 and < 1024) embed.AddField(":bangbang:  **Plugins not recognized:**", missingDashListStr);

        var missmatchDashListStr = "- " + string.Join("\n- ", Log.NewVersion.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList()) + "\n";
        if (missmatchDashListStr.Length is > 3 and < 1024) embed.AddField(":bangbang:  **Plugin version newer than DB:**", missmatchDashListStr);

        if (missingDashListStr.Length >= 1024 || missmatchDashListStr.Length >= 1024)
            embed.AddField("Attention!", "Too many unknown plugins to display them in this message. Please check the log manually.");//TODO: This is most likely abuse!

        await Logging.SendLog(originalMsgChannelId, originalMsgUserId, embed);
    }
    
    public async Task SendBaseInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null)
            throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        
        Outdated = string.Join("\r\n- ",
            Log.Outdated.Select(plugin => plugin.Link != null && plugin.Link.StartsWith("https://")
                ? $"[{plugin.DName}]({plugin.Link})"
                : $"[{plugin.DName}](https://www.google.com/search?q=lspdfr+{plugin.DName.Replace(" ", "+")})").ToList());
        
        Current = string.Join(", ", Log.Current.Select(plugin => plugin?.DName).ToList());
        Remove = string.Join("\r\n- ", ( from plug in Log.Current where (plug.State == State.BROKEN || plug.PluginType == PluginType.LIBRARY) select plug.DName ).ToList());
        Missing = string.Join(", ", Log.Missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList());
        Missmatch = string.Join(", ", Log.NewVersion.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList());
        Rph = string.Join(", ", ( from plug in Log.Current where plug.PluginType == PluginType.RPH select plug.DName ).ToList());
        
        var embedDescription = "## RPH.log Quick Info";        
        if (Log.FilePossiblyOutdated)
            embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";

        var embed = GetBaseEmbed(embedDescription);

        if (targetMessage == null) targetMessage = eventArgs!.Message;
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, Log);


        if (Missmatch.Length > 0 || Missing.Length > 0) await SendUnknownPluginsLog(cache.OriginalMessage.Channel!.Id, cache.OriginalMessage.Author!.Id);
        
        if (Outdated.Length >= 1024 || Remove.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
            if (Missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", Missing);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n>>> " + string.Join(" - ", Outdated),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n>>> " + string.Join(" - ", Remove),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl }
            };

            var overflowBuilder = new DiscordWebhookBuilder();
            overflowBuilder.AddEmbed(embed);
            if (Outdated.Length != 0) overflowBuilder.AddEmbed(embed2);
            if (Remove.Length != 0) overflowBuilder.AddEmbed(embed3);
            overflowBuilder.AddComponents([new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))]);
            
            DiscordMessage sentOverflowMessage;
            if (context != null)
                sentOverflowMessage = await context.EditResponseAsync(overflowBuilder);
            else if (eventArgs.Id == CustomIds.RphGetQuickInfo)
            {
                var responseBuilder = new DiscordInteractionResponseBuilder(overflowBuilder);
                await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage, responseBuilder);
                sentOverflowMessage = await eventArgs.Interaction.GetFollowupMessageAsync(eventArgs.Message.Id);
            }
            else
                sentOverflowMessage = await eventArgs.Interaction.EditOriginalResponseAsync(overflowBuilder);
                 
            Program.Cache.SaveProcess(sentOverflowMessage.Id, new CustomTypes.CacheTypes.ProcessCache(cache.Interaction, cache.OriginalMessage, this));
        }
        else
        {
            embed = AddCommonFields(embed);
            
            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embed);
            webhookBuilder.AddComponents(
                [
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, CustomIds.RphGetDetailedInfo, "Error Info", false, new DiscordComponentEmoji("❗")),
                    new DiscordButtonComponent(DiscordButtonStyle.Primary, CustomIds.RphGetAdvancedInfo, "Plugin Info", false, new DiscordComponentEmoji("❓")),
                    new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.RphQuickSendToUser, "Send To User", false, new DiscordComponentEmoji("📨"))
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
    
}