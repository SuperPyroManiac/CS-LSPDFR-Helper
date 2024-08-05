using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Processors;

public class SharedData
{
    public DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, ProcessCache cache, Log log) 
    {
        embed.AddField("Log uploader:", $"<@{cache.OriginalMessage.Author!.Id}>", true);
        embed.AddField("Log message:", cache.OriginalMessage.JumpLink.ToString(), true);
        embed.AddField("Elapsed time:", $"{log.ElapsedTime}ms", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    public DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
    
    public async Task SendUnknownPluginsLog(DiscordMessage msg, string dLink, List<Plugin> missing, List<Plugin> newer)
    {
        var rphLogLink = dLink != null && dLink.StartsWith("http") ? $"[here!]({dLink})" : $"here: {dLink}";
        var embed = BasicEmbeds.Warning(
            $"__Unknown Plugin / Version!__{BasicEmbeds.AddBlanks(35)}\r\n\r\n-# You can download the log {rphLogLink}\r\n");
        
        var missingDashListStr = "> - " + string.Join("\n> - ", missing.Select(plugin => $"{plugin?.Name} ({plugin?.Version})").ToList()) + "\n";
        if (missingDashListStr.Length is > 5 and < 1024) embed.Description += $"\r\n:bangbang:  **Plugins not recognized:** \r\n{missingDashListStr}";

        var missmatchDashListStr = "> - " + string.Join("\n> - ", newer.Select(plugin => $"{plugin?.Name} ({plugin?.EaVersion})").ToList()) + "\n";
        if (missmatchDashListStr.Length is > 5 and < 1024) embed.Description += $"\r\n:bangbang:  **Plugin version newer than DB:** \r\n{missmatchDashListStr}";

        if (missingDashListStr.Length >= 1024 || missmatchDashListStr.Length >= 1024)
            embed.AddField("Attention!", "Too many unknown plugins to display them in this message. Please check the log manually.");

        embed.Description += $"\r\n-# Sender: {msg.Author!.Username} ({msg.Author.Id})\r\n-# Server: {msg.Channel!.Guild.Name} ({msg.Channel.Guild.Id})\r\n-# Channel: {msg.Channel.Name} ({msg.Channel.Id})";
        
        await Logging.SendLog(embed);
    }
}