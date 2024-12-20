using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.CustomTypes.LogTypes;

public class ASILog : Log
{
    public List<Plugin> LoadedAsiFiles { get; set; } = [];
    public List<string> BrokenAsiFiles { get; set; } = [];
    public List<Plugin> FailedAsiFiles { get; set; } = [];
    public List<Plugin> Missing { get; set; } = [];
}