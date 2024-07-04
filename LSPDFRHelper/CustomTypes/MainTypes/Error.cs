using LSPDFRHelper.CustomTypes.Enums;

namespace LSPDFRHelper.CustomTypes.MainTypes;

public class Error
{
    public int Id { get; set; }
    public string Pattern { get; set; }
    public string Solution { get; set; }
    public string Description { get; set; }
    public bool StringMatch { get; set; }
    public Level Level { get; set; }
    public List<Plugin> PluginList { get; set; } = [];
}