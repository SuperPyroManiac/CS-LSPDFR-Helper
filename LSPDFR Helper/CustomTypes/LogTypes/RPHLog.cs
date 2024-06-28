using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.CustomTypes.LogTypes;

public class RPHLog : Log
{
    public string LogPath { get; set; }
    public bool LogModified { get; set; }
    public List<Error> Errors { get; set; } = [];
    public List<Plugin> Current { get; set; } = [];
    public List<Plugin> Outdated { get; set; } = [];
    public List<Plugin> Missing { get; set; } = [];
    public List<Plugin> NewVersion { get; set; } = [];
    public string GTAVersion { get; set; }
    public string RPHVersion { get; set; }
    public string LSPDFRVersion { get; set; }
}