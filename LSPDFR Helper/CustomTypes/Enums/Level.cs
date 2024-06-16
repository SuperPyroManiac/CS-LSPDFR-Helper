namespace LSPDFR_Helper.CustomTypes.Enums;

///<summary>Error types.</summary>
internal enum Level
{
    ///<summary>Special: Checks msg's in AH channels via fuzzymatch.</summary>
    PMSG,
    ///<summary>Special: Checks images text in AH channels via fuzzymatch.</summary>
    PIMG,
    ///<summary>Checks via regex - Only TS can view.</summary>
    XTRA,
    ///<summary>Checks via regex - Does not cause a crash.</summary>
    WARN,
    ///<summary>Checks via regex - Can cause a crash.</summary>
    SEVERE,
    ///<summary>Checks via regex - High priority, other errors will not show when this is present!</summary>
    CRITICAL
}