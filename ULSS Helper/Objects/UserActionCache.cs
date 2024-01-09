using DSharpPlus.Entities;

namespace ULSS_Helper.Objects;

/// <summary>
/// Mainly used to store information during the process where a user enters a command with parameters, then gets a modal form and submits the form.
/// </summary>
internal class UserActionCache : Cache
{
    internal DiscordInteraction Interaction { get; }
    internal Plugin Plugin { get; }
    internal Error Error { get; }
    internal DiscordUser User { get; }

    internal UserActionCache(DiscordInteraction interaction, Plugin plugin)
    {
        Interaction = interaction;
        Plugin = plugin;
    }

    internal UserActionCache(DiscordInteraction interaction, Error error)
    {
        Interaction = interaction;
        Error = error;
    }
    
    internal UserActionCache(DiscordInteraction interaction, DiscordUser user)
    {
        Interaction = interaction;
        User = user;
    }
}