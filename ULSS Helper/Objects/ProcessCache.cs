using DSharpPlus.Entities;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;

namespace ULSS_Helper.Objects;

/// <summary>
/// Used to store information during a log analysis process for a specific DiscordMessage. This allows accessing the Process object instances in the (possible) following chain of bot responses related to the same OriginalMessage.
/// </summary>
internal class ProcessCache : Cache
{
    internal DiscordInteraction Interaction { get; private set; }
    internal DiscordMessage OriginalMessage { get; private set; }
    internal ELSProcess ElsProcess { get; private set; }
    internal RPHProcess RphProcess { get; private set; }
    internal ASIProcess AsiProcess { get; private set; }
    internal SHVDNProcess ShvdnProcess { get; private set; }

    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, ELSProcess elsProcess) : base()
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        ElsProcess = elsProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, RPHProcess rphProcess) : base()
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        RphProcess = rphProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, ASIProcess asiProcess) : base()
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        AsiProcess = asiProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, SHVDNProcess shvdnProcess) : base()
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        ShvdnProcess = shvdnProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage) : base()
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
    }

    internal ProcessCache Update(ProcessCache newCache)
    {
        if ((this.OriginalMessage ?? newCache.OriginalMessage) != null && this.OriginalMessage.Id != newCache.OriginalMessage.Id)
            throw new InvalidOperationException("Cannot update ProcessCache that belongs to a different message!");
        
        this.Interaction = newCache.Interaction ?? this.Interaction;
        this.OriginalMessage = newCache.OriginalMessage ?? this.OriginalMessage;
        this.ElsProcess = newCache.ElsProcess ?? this.ElsProcess;
        this.RphProcess = newCache.RphProcess ?? this.RphProcess;
        this.AsiProcess = newCache.AsiProcess ?? this.AsiProcess;
        this.ShvdnProcess = newCache.ShvdnProcess ?? this.ShvdnProcess;
        base.Update();
        return this;
    }
}