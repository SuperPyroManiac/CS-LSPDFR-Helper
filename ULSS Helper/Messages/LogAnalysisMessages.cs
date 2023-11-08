
using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class LogAnalysisMessages
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless";
    internal static ulong logUploaderUserId;
    internal static string logMessageLink;

    internal static DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed) 
    {
        embed.AddField("Log uploader:", $"<@{logUploaderUserId}>", true);
        embed.AddField("Log message:", logMessageLink, true);
        embed.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal static DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}