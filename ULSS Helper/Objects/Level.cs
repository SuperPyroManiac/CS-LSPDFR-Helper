using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

public enum Level
{
    [ChoiceName("WARN")]
    WARN,
    [ChoiceName("SEVERE")]
    SEVERE,
    [ChoiceName("CRITICAL")]
    CRITICAL
}