using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal static class BasicEmbeds
{
    internal static DiscordEmbedBuilder Error(string msg, bool bold = false)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":no_entry:  {msg}",
            Color = DiscordColor.Red
        };
        if (bold) embed.Description = $"### :no_entry:  {msg}";
        return embed;
    }

    internal static DiscordEmbedBuilder Warning(string msg, bool bold = false)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":warning:  {msg}",
            Color = DiscordColor.Gold
        };
        if (bold) embed.Description = $"### :warning:  {msg}";
        return embed;
    }

    internal static DiscordEmbedBuilder Info(string msg, bool bold = false)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":grey_exclamation:  {msg}",
            Color = DiscordColor.DarkBlue
        };
        if (bold) embed.Description = $"### :grey_exclamation:  {msg}";
        return embed;
    }

    internal static DiscordEmbedBuilder Success(string msg, bool bold = false)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":white_check_mark:  {msg}",
            Color = DiscordColor.SapGreen
        };
        if (bold) embed.Description = $"### :white_check_mark:  {msg}";
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