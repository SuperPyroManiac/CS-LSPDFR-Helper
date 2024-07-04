// ReSharper disable InconsistentNaming
namespace LSPDFRHelper.CustomTypes.SpecialTypes;

public class GlobalSettings
{
    public string Id { get; set; }
    public ulong ServerId { get; set; }
    public bool AHStatus { get; set; }
    public string TsIconUrl { get; set; }
    public ulong TsRoleId { get; set; }
    public ulong AutoHelperChId { get; set; }
    public ulong SupportChId { get; set; }
    public ulong MonitorChId { get; set; }
    public ulong BotLogChId { get; set; }
    public ulong PublicLogChId { get; set; }
    public ulong ErrorLogChId { get; set; }
    public ulong ReportChId { get; set; }
    public ulong StaffContactChId { get; set; }
    public ulong AnnounceChId { get; set; }
}