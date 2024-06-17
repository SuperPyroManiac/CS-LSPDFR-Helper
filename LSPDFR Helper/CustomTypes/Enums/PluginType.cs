namespace LSPDFR_Helper.CustomTypes.Enums;

///<summary>Types of plugins.</summary>
internal enum PluginType
{
    ///<summary>LSPDFR plugins.</summary>
    LSPDFR,
    ///<summary>RagePluginHook plugins.</summary>
    RPH,
    ///<summary>ASI scripts.</summary>
    ASI,
    ///<summary>ScriptHookV scripts/</summary>
    SHV,
    ///<summary>ScriptHookVDotNet scripts.</summary>
    SHVDN,
    ///<summary>Libraries, ex: RageNativeUI or PyroCommon</summary>
    LIBRARY
}