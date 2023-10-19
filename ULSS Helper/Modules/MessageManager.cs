using DSharpPlus.Entities;

namespace ULSS_Helper.Modules;

internal static class MessageManager
{
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
}