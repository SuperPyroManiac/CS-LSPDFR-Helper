namespace ULSS_Helper;

public class AnalyzedRphLog
{
    public List<Plugin?> Current { get; set; }
    public List<Plugin?> Outdated { get; set; }
    public List<Plugin?> Broken { get; set; }
    public List<Plugin?> Library { get; set; }
    public List<Plugin?> Missing { get; set; }
    public List<Plugin> Missmatch { get; set; }
    
    public List<Error?> Errors { get; set; }

    public string GTAVersion { get; set; }
    public string RPHVersion { get; set; }
    public string LSPDFRVersion { get; set; }
    public ulong MsgId { get; set; }
}