using DSharpPlus.Entities;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;

namespace ULSS_Helper.Objects;

internal class ProcessCache
{
    internal DiscordInteraction Interaction { get; }
    internal DiscordMessage OriginalMessage { get; }
    internal ELSProcess? ElsProcess { get; }
    internal RPHProcess? RphProcess { get; }
    internal ASIProcess? AsiProcess { get; }
    internal SHVDNProcess? ShvdnProcess { get; }

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
}