namespace LSPDFRHelper.CustomTypes.LogTypes;

public class Log
{
    public ulong MsgId { get; set; }
    public string DownloadLink { get; set; }
    public string ElapsedTime { get; set; }
    public DateTime ValidaterStartedAt { get; } = DateTime.Now;
    public DateTime ValidaterCompletedAt { get; set; }
}