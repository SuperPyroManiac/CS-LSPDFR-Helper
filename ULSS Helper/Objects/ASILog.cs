namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class ASILog : Log
{
    public List<string> LoadedAsiFiles { get; set; }
    public List<string> FailedAsiFiles { get; set; }
    public List<string> OutdatedAsiFiles { get; set; }
    public List<string> AllAsiFiles { get; set; }
}