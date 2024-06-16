using LSPDFR_Helper.CustomTypes.Enums;

namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class Plugin
{
    internal string Name { get; set; }
    internal string DName { get; set; }
    internal string Description { get; set; }
    internal string Version { get; set; }
    internal string EaVersion { get; set; }
    internal int Id { get; set; }
    internal string Link { get; set; }
    internal PluginType PluginType { get; set; }
    internal State State { get; set; }
}