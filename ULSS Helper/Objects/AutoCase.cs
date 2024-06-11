namespace ULSS_Helper.Objects;

public class AutoCase
{
    public string CaseID { get; set; }
    public string OwnerID { get; set; }
    public string ChannelID { get; set; }
    public string RequestID { get; set; }
    public int Solved { get; set; }
    public int TsRequested { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpireDate { get; set; }
}