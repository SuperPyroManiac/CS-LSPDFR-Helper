namespace LSPDFRHelper.CustomTypes.MainTypes;

public class AutoCase
{
    public string CaseId { get; set; }
    public ulong OwnerId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong RequestId { get; set; }
    public bool Solved { get; set; }
    public bool TsRequested { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
}