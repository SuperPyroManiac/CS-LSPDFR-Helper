namespace ULSS_Helper.Objects;

public enum Level
{
    PMSG,
    PIMG,
    XTRA,
    WARN,
    SEVERE,
    CRITICAL
}

public enum State
{
    LSPDFR,
    RPH,
    ASI,
    SHV,
    SHVDN,
    LIB,
    EXTERNAL,
    BROKEN,
    IGNORE
}

public enum DbOperation
{
    CREATE,
    READ,
    UPDATE,
    DELETE
}
