using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal static class BasicEmbeds
{
    internal static DiscordEmbedBuilder Error(string msg)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":no_entry:  {msg}",
            Color = DiscordColor.Red
        };
        return embed;
    }

    internal static DiscordEmbedBuilder Warning(string msg)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":warning:  {msg}",
            Color = DiscordColor.Gold
        };
        return embed;
    }

    internal static DiscordEmbedBuilder Info(string msg)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":grey_exclamation:  {msg}",
            Color = DiscordColor.DarkBlue
        };
        return embed;
    }

    internal static DiscordEmbedBuilder Generic(string msg, DiscordColor color)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = msg,
            Color = color
        };
        return embed;
    }
}