namespace LSPDFRHelper.CustomTypes.Enums;

///<summary>Types of plugins.</summary>
public enum PluginType
{
    ///<summary>LSPDFR plugins.</summary>
    LSPDFR = 1,
    ///<summary>RagePluginHook plugins.</summary>
    RPH = 2,
    ///<summary>ASI scripts.</summary>
    ASI = 3,
    ///<summary>ScriptHookV scripts/</summary>
    SHV = 4,
    ///<summary>ScriptHookVDotNet scripts.</summary>
    SHVDN = 5,
    ///<summary>Libraries, ex: RageNativeUI or PyroCommon</summary>
    LIBRARY = 6
}