using DSharpPlus.Entities;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Objects;

internal class ProcessCache
{
    internal DiscordInteraction Interaction { get; }
    internal DiscordMessage OriginalMessage { get; }
    internal ELSProcess? ElsProcess { get; }
    internal RPHProcess? RphProcess { get; }

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
}