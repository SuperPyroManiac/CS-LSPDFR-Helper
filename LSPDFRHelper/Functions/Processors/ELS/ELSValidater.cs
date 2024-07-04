using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.LogTypes;

namespace LSPDFRHelper.Functions.Processors.ELS;

public class ELSValidater
{
    public static async Task<ELSLog> Run(string attachmentUrl)
    {
        var log = new ELSLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        log.ElsVersion = new Regex(@"VER\s+\[ ([0-9.]+) \]").Match(wholeLog).Groups[1].Value;
        log.AdvancedHookVFound = new Regex(@"\s+\(OK\) AdvancedHookV\.dll file found and successfully initialized\.").Match(wholeLog).Success;

        foreach (Match match in new Regex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(ER\) Model specified is not valid, discarding file\.").Matches(wholeLog)) 
            if (!log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value))) log.InvalidElsVcfFiles.Add(match.Groups[1].Value);
        
        foreach (Match match in new Regex(@"<FILEV> Found file: \[ (.*) \]\.\r\n<FILEV> Verifying vehicle model name \[ .* \]\.\r\n\s+\(OK\) Is valid vehicle model, assigned VMID \[ .* \]\.\r\n\s+Parsing file. \*A crash before all clear indicates faulty VCF\.\*\r\n\s+VCF Description: .*\r\n\s+VCF Author: .*(\r\n\s+\(OK\) Collected data from: '\w+'\.)+\r\n\s+\(OK\) ALL CLEAR -- Configuration file processed\.").Matches(wholeLog))
            if (!log.ValidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)) && !log.InvalidElsVcfFiles.Any(x => x.Equals(match.Groups[1].Value)))
                log.ValidElsVcfFiles.Add(match.Groups[1].Value);

        if (new Regex(@"\s+\(OK\) Collected data from: '\w+'\.\r\n$").Match(wholeLog).Success)
            log.FaultyVcfFile = new Regex(@"<FILEV> Found file: \[ (.+\.xml) \]\.").Matches(wholeLog)[^1].Groups[1].Value;
        
        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).Milliseconds.ToString();

        return log;
    }
}