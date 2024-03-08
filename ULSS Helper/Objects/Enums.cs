using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

public enum Level
{
    [ChoiceName("PMSG")] PMSG,
    [ChoiceName("PIMG")] PIMG,
    [ChoiceName("XTRA")] XTRA,
    [ChoiceName("WARN")] WARN,
    [ChoiceName("SEVERE")] SEVERE,
    [ChoiceName("CRITICAL")] CRITICAL
}

public enum State
{
    [ChoiceName("LSPDFR")] LSPDFR,
    [ChoiceName("EXTERNAL")] EXTERNAL,
    [ChoiceName("BROKEN")] BROKEN,
    [ChoiceName("LIB")] LIB,
    [ChoiceName("IGNORE")] IGNORE
}

public enum DbOperation
{
    CREATE,
    READ,
    UPDATE,
    DELETE
}
