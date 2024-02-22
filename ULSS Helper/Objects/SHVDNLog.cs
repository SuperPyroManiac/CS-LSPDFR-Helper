namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class SHVDNLog : Log
{
    public List<string> FrozenScripts { get; set; }
    public List<string> ScriptDepends { get; set; }
}