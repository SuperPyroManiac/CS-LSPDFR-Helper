using DSharpPlus.Entities;

namespace ULSS_Helper.Objects;

internal class InteractionCache
{
    internal DiscordInteraction Interaction { get; }
    internal Plugin Plugin { get; }
    internal Error Error { get; }

    internal InteractionCache(DiscordInteraction interaction, Plugin plugin)
    {
        Interaction = interaction;
        Plugin = plugin;
    }

    internal InteractionCache(DiscordInteraction interaction, Error error)
    {
        Interaction = interaction;
        Error = error;
    }
}