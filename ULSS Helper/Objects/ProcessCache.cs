using DSharpPlus.Entities;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;

namespace ULSS_Helper.Objects;

internal class ProcessCache
{
    internal DiscordInteraction Interaction { get; private set; }
    internal DiscordMessage OriginalMessage { get; private set; }
    internal ELSProcess ElsProcess { get; private set; }
    internal RPHProcess RphProcess { get; private set; }
    internal ASIProcess AsiProcess { get; private set; }
    internal SHVDNProcess ShvdnProcess { get; private set; }

    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, ELSProcess elsProcess)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        ElsProcess = elsProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, RPHProcess rphProcess)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        RphProcess = rphProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, ASIProcess asiProcess)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        AsiProcess = asiProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage, SHVDNProcess shvdnProcess)
    {
        Interaction = interaction;
        OriginalMessage = originalMessage;
        ShvdnProcess = shvdnProcess;
    }
    internal ProcessCache(DiscordInteraction interaction, DiscordMessage originalMessage)
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
        return this;
    }
}