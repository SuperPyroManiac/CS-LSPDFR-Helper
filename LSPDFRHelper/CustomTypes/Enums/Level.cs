namespace LSPDFRHelper.CustomTypes.Enums;

///<summary>Error types.</summary>
public enum Level
{//TODO: Add a track type, this one informs us when a match is found in a log in the bot log channel.
    ///<summary>Special: Checks msg's in AH channels via fuzzymatch.</summary>
    PMSG = 1,
    ///<summary>Special: Checks images text in AH channels via fuzzymatch.</summary>
    PIMG = 2,
    ///<summary>Checks via regex - Only TS can view.</summary>
    XTRA = 3,
    ///<summary>Checks via regex - Does not cause a crash.</summary>
    WARN = 4,
    ///<summary>Checks via regex - Can cause a crash.</summary>
    SEVERE = 5,
    ///<summary>Checks via regex - High priority, other errors will not show when this is present!</summary>
    CRITICAL = 6
}