using LSPDFR_Helper.CustomTypes.Enums;

namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class Error
{
    internal int Id { get; set; }
    internal string Pattern { get; set; }
    internal string Solution { get; set; }
    internal string Description { get; set; }
    internal bool StringMatch { get; set; }
    internal Level Level { get; set; }
}