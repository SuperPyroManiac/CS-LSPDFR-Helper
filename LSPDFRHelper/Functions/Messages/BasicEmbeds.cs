﻿using DSharpPlus.Entities;

namespace LSPDFRHelper.Functions.Messages;

public static class BasicEmbeds
{
    public static DiscordEmbedBuilder Error(string msg, bool bold = true)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":no_entry:  {msg}",
            Color = DiscordColor.Red
        };
        if (bold) embed.Description = $"### :no_entry:  {msg}";
        return embed;
    }

    public static DiscordEmbedBuilder Warning(string msg, bool bold = true)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":warning:  {msg}",
            Color = DiscordColor.Gold
        };
        if (bold) embed.Description = $"### :warning:  {msg}";
        return embed;
    }

    public static DiscordEmbedBuilder Info(string msg, bool bold = true)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $":grey_exclamation:  {msg}",
            Color = DiscordColor.DarkBlue
        };
        if (bold) embed.Description = $"### :grey_exclamation:  {msg}";
        return embed;
    }

    public static DiscordEmbedBuilder Success(string msg, bool bold = true)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = $"{DiscordEmoji.FromName(Program.Client, ":yes:")}  {msg}",
            Color = DiscordColor.DarkGreen
        };
        if (bold) embed.Description = $"### {DiscordEmoji.FromName(Program.Client, ":yes:")}  {msg}";
        return embed;
    }

    public static DiscordEmbedBuilder Generic(string msg, DiscordColor color)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = msg,
            Color = color
        };
        return embed;
    }
    
    public static DiscordEmbedBuilder Ts(string msg, DiscordEmbedBuilder.EmbedFooter footer)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = msg,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl },
            Footer = footer
        };
        return embed;
    }
    
    public static DiscordEmbedBuilder Public(string msg)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = msg,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {Text = "AutoHelper - Generated By Discord.gg/ulss"}
        };
        return embed;
    }

    public static string AddBlanks(int count)
    {
        return string.Join("", Enumerable.Repeat("\u200b ", count));
    }
}