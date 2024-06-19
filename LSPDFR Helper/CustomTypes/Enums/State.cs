namespace LSPDFR_Helper.CustomTypes.Enums;

///<summary>State of the plugin.</summary>
public enum State
{
    ///<summary>Normal and working.</summary>
    NORMAL,
    ///<summary>Normal but not downloaded from LCPDFR.com</summary>
    EXTERNAL,
    ///<summary>Does not work, should be removed.</summary>
    BROKEN,
    ///<summary>SPECIAL: Requires Pyro's approval!</summary>
    IGNORE
}