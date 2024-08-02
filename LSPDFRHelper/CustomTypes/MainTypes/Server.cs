// ReSharper disable InconsistentNaming
namespace LSPDFRHelper.CustomTypes.MainTypes;

public class Server
{
    public ulong ServerId { get; set; }
    public string Name { get; set; }
    public ulong OwnerId { get; set; }
    public bool Enabled { get; set; }
    public bool Blocked { get; set; }
    public bool AhEnabled { get; set; }
    public ulong AutoHelperChId { get; set; }
    public ulong MonitorChId { get; set; }
    public ulong AnnounceChId { get; set; }
    public ulong ManagerRoleId { get; set; }
}