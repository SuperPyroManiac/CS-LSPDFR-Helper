using System.Text.RegularExpressions;
using LSPDFR_Helper.CustomTypes.LogTypes;

namespace LSPDFR_Helper.Functions.Processors.ELS;

// ReSharper disable InconsistentNaming
public class ELSValidater
{
    public static async Task<ELSLog> Run(string attachmentUrl)
    {
        var log = new ELSLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        log.ElsVersion = new Regex(@"VER\s+\[ ([0-9.]+) \]").Match(wholeLog).Groups[1].Value;
        log.AdvancedHookVFound = new Regex(@"\s+\(OK\) AdvancedHookV\.dll file found and successfully initialized\.").Match(wholeLog).Success;
        log.VcfContainer = new Regex(@"<VCONT> Attempting to open VCF container at: \[ \.(.*) \]\.\r\n\s+\(OK\) VCF container found, scanning for files\.", RegexOptions.Multiline).Match(wholeLog).Groups[1].Value.Replace("//", "/");

        foreach (Match match in new Regex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(ER\) Model specified is not valid, discarding file\.").Matches(wholeLog)) 
            if (!log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value))) log.InvalidElsVcfFiles.Add(match.Groups[1].Value);
        
        foreach (Match match in new Regex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(OK\) Is valid vehicle model, assigned VMID \[ .* \]\.\r\n\s+Parsing file. \*A crash before all clear indicates faulty VCF\.\*\r\n\s+VCF Description: .*\r\n\s+VCF Author: .*(\r\n\s+\(OK\) Collected data from: '\w+'\.)+\r\n\s+\(OK\) ALL CLEAR -- Configuration file processed\.").Matches(wholeLog))
            if (!log.ValidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)) && !log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)))
                log.ValidElsVcfFiles.Add(match.Groups[1].Value);

        if (new Regex(@"\s+\(OK\) Collected data from: '\w+'\.\r\n$").Match(wholeLog).Success)
            log.FaultyVcfFile = new Regex(@"<FILEV> Found file: \[ (.+\.xml) \]\.").Matches(wholeLog)[^1].Groups[1].Value;

        var matchTotalAmountVcfs = new Regex(@"<FILEV> Vehicle information retrieved\. Loaded \[ (\d+) \] ELS-enabled model\(s\)\.").Match(wholeLog);
        if (matchTotalAmountVcfs.Success) log.TotalAmountElsModels = int.Parse(matchTotalAmountVcfs.Groups[1].Value);
        else { log.TotalAmountElsModels = null; }
        
        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).Milliseconds.ToString();

        return log;
    }
}