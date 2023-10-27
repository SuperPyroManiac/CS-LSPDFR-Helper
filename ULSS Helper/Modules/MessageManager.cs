using DSharpPlus.Entities;
using static ULSS_Helper.Modules.ContextManager;

namespace ULSS_Helper.Modules;

internal static class MessageManager
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless";
    
    internal static DiscordEmbedBuilder Error(string msg)
    {
        var message = new DiscordEmbedBuilder
        {
            Description = $":no_entry:  {msg}",
            Color = DiscordColor.Red
        };
        return message;
    }
    
    internal static DiscordEmbedBuilder Warning(string msg)
    {
        var message = new DiscordEmbedBuilder
        {
            Description = $":warning:  {msg}",
            Color = DiscordColor.Gold
        };
        return message;
    }
    
    internal static DiscordEmbedBuilder Info(string msg)
    {
        var message = new DiscordEmbedBuilder
        {
            Description = $":grey_exclamation:  {msg}",
            Color = DiscordColor.DarkBlue
        };
        return message;
    }
    
    internal static DiscordEmbedBuilder Generic(string msg, DiscordColor color)
    {
        var message = new DiscordEmbedBuilder
        {
            Description = msg,
            Color = color
        };
        return message;
    }
    
    //=======================================================================================
    //Log Checker Messages!
    //=======================================================================================
    
    internal static DiscordEmbedBuilder GetBaseLogInfoMessage(string description)
    {
        if (Settings.GTAVer == log.GTAVersion) GTAver = "\u2713";
        if (Settings.LSPDFRVer == log.LSPDFRVersion) LSPDFRver = "\u2713";
        if (Settings.RPHVer == log.RPHVersion) RPHver = "\u2713";

        return new DiscordEmbedBuilder(){
            Description = description,
            Color = DiscordColor.Gold,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter()
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
        if (current.Length == 0 && outdated.Length != 0 || broken.Length != 0) current = "**None**";

        if (outdated.Length > 0) message.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
        if (broken.Length > 0) message.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);
        if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
    
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0) message.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion)) message.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
        if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0) message.AddField(":green_circle:     **No installed plugins!**", "- Can't have plugin issues if you don't got any!");

        return message;
    }
}