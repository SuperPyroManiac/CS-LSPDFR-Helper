using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

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