using DSharpPlus.SlashCommands;

namespace ULSS_Helper;

public enum State
{
    [ChoiceName("LSPDFR")]
    LSPDFR,
    [ChoiceName("EXTERNAL")]
    EXTERNAL,
    [ChoiceName("BROKEN")]
    BROKEN,
    [ChoiceName("LIB")]
    LIB
}