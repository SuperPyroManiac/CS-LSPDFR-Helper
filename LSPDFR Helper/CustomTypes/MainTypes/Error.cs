using LSPDFR_Helper.CustomTypes.Enums;

namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class Error
{
    public int Id { get; set; }
    public string Pattern { get; set; }
    public string Solution { get; set; }
    public string Description { get; set; }
    public Level Level { get; set; }
}