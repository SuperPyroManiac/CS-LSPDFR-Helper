using DSharpPlus.Entities;

namespace ULSS_Helper.Objects;

/// <summary>
/// Mainly used to store information during the process where a user enters a command with parameters, then gets a modal form and submits the form.
/// </summary>
public class UserActionCache : Cache
{
    public DiscordInteraction Interaction { get; }
    public Plugin Plugin { get; }
    public Error Error { get; }
    public DiscordUser User { get; }
    public DiscordMessage Msg { get; }

    public UserActionCache(DiscordInteraction interaction, Plugin plugin, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Plugin = plugin;
        Msg = msg;
    }

    public UserActionCache(DiscordInteraction interaction, Error error, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Error = error;
        Msg = msg;
    }
    
    public UserActionCache(DiscordInteraction interaction, DiscordUser user, DiscordMessage msg = null)
    {
        Interaction = interaction;
        User = user;
        Msg = msg;
    }
}