namespace ULSS_Helper.Objects;

public class ASILog : Log
{
    public List<string>? LoadedAsiFiles { get; set; }
    public List<string>? FailedAsiFiles { get; set; }
}