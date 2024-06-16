using LSPDFR_Helper.CustomTypes.Enums;
using Type = LSPDFR_Helper.CustomTypes.Enums.Type;

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
    internal Type Type { get; set; }
    internal State State { get; set; }
}