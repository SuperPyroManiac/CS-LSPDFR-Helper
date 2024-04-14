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
    [ChoiceName("RPH")] RPH,
    [ChoiceName("ASI")] ASI,
    [ChoiceName("SHV")] SHV,
    [ChoiceName("SHVDN")] SHVDN,
    [ChoiceName("LIB")] LIB,
    [ChoiceName("EXTERNAL")] EXTERNAL,
    [ChoiceName("BROKEN")] BROKEN,
    [ChoiceName("IGNORE")] IGNORE
}

public enum DbOperation
{
    CREATE,
    READ,
    UPDATE,
    DELETE
}
