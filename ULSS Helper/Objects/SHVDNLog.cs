namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class SHVDNLog : Log
{
    public List<string> Scripts { get; set; }
    public List<string> MissingDepends { get; set; }
}