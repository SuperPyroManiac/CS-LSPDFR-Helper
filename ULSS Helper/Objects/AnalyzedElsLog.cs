namespace ULSS_Helper.Objects;

public class AnalyzedElsLog
{
    public string? ElsVersion { get; set; }
    public bool AdvancedHookVFound { get; set; }
    public string? VcfContainer { get; set; }
    public List<string>? ValidElsVcfFiles { get; set; }
    public List<string>? InvalidElsVcfFiles { get; set; }
    public string? FaultyVcfFile { get; set; }
    public int? TotalAmountElsModels { get; set; }
    public ulong MsgId { get; set; }
}