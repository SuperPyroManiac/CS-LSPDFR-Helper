using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.CustomTypes.CacheTypes;

/// <summary>
/// Stored info and types from command interactions.
/// </summary>
internal class InteractionCache
{
    internal DateTime Expire = DateTime.Now.AddMinutes(15);
    internal DiscordInteraction Interaction { get; }
    internal Plugin Plugin { get; }
    internal Error Error { get; }
    internal User User { get; }
    internal DiscordMessage Msg { get; }

    internal InteractionCache(DiscordInteraction interaction, Plugin plugin, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Plugin = plugin;
        Msg = msg;
    }

    internal InteractionCache(DiscordInteraction interaction, Error error, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Error = error;
        Msg = msg;
    }
    
    internal InteractionCache(DiscordInteraction interaction, User user, DiscordMessage msg = null)
    {
        Interaction = interaction;
        User = user;
        Msg = msg;
    }
}