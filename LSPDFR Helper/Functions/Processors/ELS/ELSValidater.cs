using System.Text.RegularExpressions;
using LSPDFR_Helper.CustomTypes.LogTypes;

namespace LSPDFR_Helper.Functions.Processors.ELS;

public partial class ELSValidater
{
    public static async Task<ELSLog> Run(string attachmentUrl)
    {
        var log = new ELSLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        log.ElsVersion = ElsVersionReg().Match(wholeLog).Groups[1].Value;
        log.AdvancedHookVFound = AdvancedHookVFoundReg().Match(wholeLog).Success;
        log.VcfContainer = VcfContainerReg().Match(wholeLog).Groups[1].Value.Replace("//", "/");

        foreach (Match match in InvalidElsVcfFilesReg().Matches(wholeLog)) 
            if (!log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value))) log.InvalidElsVcfFiles.Add(match.Groups[1].Value);
        
        foreach (Match match in ValidElsVcfFilesReg().Matches(wholeLog))
            if (!log.ValidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)) && !log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)))
                log.ValidElsVcfFiles.Add(match.Groups[1].Value);

        if ( DetectFaultReg().Match(wholeLog).Success)
            log.FaultyVcfFile = FaultyVcfFileReg().Matches(wholeLog)[^1].Groups[1].Value;

        var matchTotalAmountVcfs = matchTotalAmountVcfsReg().Match(wholeLog);
        if (matchTotalAmountVcfs.Success) log.TotalAmountElsModels = int.Parse(matchTotalAmountVcfs.Groups[1].Value);
        else { log.TotalAmountElsModels = null; }
        
        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).Milliseconds.ToString();

        return log;
    }

    [GeneratedRegex(@"VER\s+\[ ([0-9.]+) \]")]
    private static partial Regex ElsVersionReg();
    [GeneratedRegex(@"\s+\(OK\) AdvancedHookV\.dll file found and successfully initialized\.")]
    private static partial Regex AdvancedHookVFoundReg();
    [GeneratedRegex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(ER\) Model specified is not valid, discarding file\.")]
    private static partial Regex InvalidElsVcfFilesReg();
    [GeneratedRegex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(OK\) Is valid vehicle model, assigned VMID \[ .* \]\.\r\n\s+Parsing file. \*A crash before all clear indicates faulty VCF\.\*\r\n\s+VCF Description: .*\r\n\s+VCF Author: .*(\r\n\s+\(OK\) Collected data from: '\w+'\.)+\r\n\s+\(OK\) ALL CLEAR -- Configuration file processed\.")]
    private static partial Regex ValidElsVcfFilesReg();
    [GeneratedRegex(@"<FILEV> Found file: \[ (.+\.xml) \]\.")]
    private static partial Regex FaultyVcfFileReg();
    [GeneratedRegex(@"<FILEV> Vehicle information retrieved\. Loaded \[ (\d+) \] ELS-enabled model\(s\)\.")]
    private static partial Regex matchTotalAmountVcfsReg();
    [GeneratedRegex(@"<VCONT> Attempting to open VCF container at: \[ \.(.*) \]\.\r\n\s+\(OK\) VCF container found, scanning for files\.", RegexOptions.Multiline)]
    private static partial Regex VcfContainerReg();
    [GeneratedRegex(@"\s+\(OK\) Collected data from: '\w+'\.\r\n$")]
    private static partial Regex DetectFaultReg();
}