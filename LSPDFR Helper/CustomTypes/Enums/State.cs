namespace LSPDFR_Helper.CustomTypes.Enums;

///<summary>State of the plugin.</summary>
public enum State
{
    ///<summary>Normal and working.</summary>
    NORMAL = 1,
    ///<summary>Normal but not downloaded from LCPDFR.com</summary>
    EXTERNAL = 2,
    ///<summary>Does not work, should be removed.</summary>
    BROKEN = 3,
    ///<summary>SPECIAL: Requires Pyro's approval!</summary>
    IGNORE = 4
}