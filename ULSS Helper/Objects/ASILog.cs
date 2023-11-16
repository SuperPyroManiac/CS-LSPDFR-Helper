namespace ULSS_Helper.Objects;

public class ASILog : Log
{
    public List<string>? LoadedASIFiles { get; set; }
    public List<string>? FailedASIFiles { get; set; }
}