using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.CustomTypes.CacheTypes;

/// <summary>
/// Stored info and types from command interactions.
/// </summary>
public class InteractionCache
{
    public DateTime Expire = DateTime.Now.AddMinutes(15);
    public DiscordInteraction Interaction { get; }
    public Plugin Plugin { get; }
    public Error Error { get; }
    public User User { get; }
    public Server Server { get; }
    public DiscordMessage Msg { get; }

    public InteractionCache(DiscordInteraction interaction, Plugin plugin, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Plugin = plugin;
        Msg = msg;
    }

    public InteractionCache(DiscordInteraction interaction, Error error, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Error = error;
        Msg = msg;
    }
    
    public InteractionCache(DiscordInteraction interaction, User user, DiscordMessage msg = null)
    {
        Interaction = interaction;
        User = user;
        Msg = msg;
    }
    
    public InteractionCache(DiscordInteraction interaction, Server server, DiscordMessage msg = null)
    {
        Interaction = interaction;
        Server = server;
        Msg = msg;
    }
}