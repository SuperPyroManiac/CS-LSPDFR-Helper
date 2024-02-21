namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class SHVDNLog : Log
{
    public List<string> ScriptsCausingFreeze { get; set; }
    public List<string> MissingFiles { get; set; }
}