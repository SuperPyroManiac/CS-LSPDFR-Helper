using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.LogTypes;

namespace LSPDFR_Helper.Functions.Processors;

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
}