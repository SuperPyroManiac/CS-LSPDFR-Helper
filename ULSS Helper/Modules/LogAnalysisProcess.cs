
using DSharpPlus.Entities;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules;

internal class LogAnalysisProcess
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/b69077cfafb6856a0752c863e1bb87f0.webp?size=128&quality=lossless";
    internal Guid Guid { get; }
    
    public LogAnalysisProcess()
    {
        Guid = Guid.NewGuid();
    }

    internal static DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, ulong messageId) 
    {
        ProcessCache cache = Program.Cache.GetProcessCache(messageId);

        embed.AddField("Log uploader:", $"<@{cache.OriginalMessage.Author.Id}>", true);
        embed.AddField("Log message:", cache.OriginalMessage.JumpLink.ToString(), true);
        embed.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal static DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}