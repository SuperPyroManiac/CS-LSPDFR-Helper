namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class RPHLog : Log
{
    public bool FilePossiblyOutdated { get; set; }
    public List<Plugin> RPHPlugin { get; set; }
    public List<Plugin> Current { get; set; }
    public List<Plugin> Outdated { get; set; }
    public List<Plugin> Broken { get; set; }
    public List<Plugin> Library { get; set; }
    public List<Plugin> Missing { get; set; }
    public List<Plugin> Missmatch { get; set; }
    public List<Error> Errors { get; set; }
    public List<Plugin> MissingDepend { get; set; }
    public List<string> IncorrectScripts { get; set; }
    public List<string> IncorrectPlugins { get; set; }
    public List<string> IncorrectLibs { get; set; }
    public List<string> IncorrectOther { get; set; }
    public string GTAVersion { get; set; }
    public string RPHVersion { get; set; }
    public string LSPDFRVersion { get; set; }
    public string FilePath { get; set; }
}