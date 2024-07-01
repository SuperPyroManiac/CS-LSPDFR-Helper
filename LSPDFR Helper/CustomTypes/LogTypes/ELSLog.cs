namespace LSPDFR_Helper.CustomTypes.LogTypes;

// ReSharper disable InconsistentNaming
public class ELSLog : Log
{
    public string ElsVersion { get; set; }
    public bool AdvancedHookVFound { get; set; }
    public List<string> ValidElsVcfFiles { get; set; } = [];
    public List<string> InvalidElsVcfFiles { get; set; } = [];
    public string FaultyVcfFile { get; set; }
}
