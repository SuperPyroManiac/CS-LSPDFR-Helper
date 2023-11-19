namespace ULSS_Helper.Objects;

// ReSharper disable InconsistentNaming
public class ELSLog : Log
{
    public string ElsVersion { get; set; }
    public bool AdvancedHookVFound { get; set; }
    public string VcfContainer { get; set; }
    public List<string> ValidElsVcfFiles { get; set; }
    public List<string> InvalidElsVcfFiles { get; set; }
    public string FaultyVcfFile { get; set; }
    public int? TotalAmountElsModels { get; set; }
}
