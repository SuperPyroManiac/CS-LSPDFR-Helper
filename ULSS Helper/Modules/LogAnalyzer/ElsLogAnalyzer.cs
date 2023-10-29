using System.Net;
using System.Text.RegularExpressions;

namespace ULSS_Helper.Modules;

public class ElsLogAnalyzer
{
    internal static AnalyzedElsLog Run(string fileName)
    {
        using var client = new WebClient();
        client.DownloadFile(
            fileName,
            Path.Combine(
                Directory.GetCurrentDirectory(), 
                "ELSLogs", 
                Settings.ElsLogNamer()
            )
        );
        var log = new AnalyzedElsLog();
        var wholeLog = File.ReadAllText(Settings.ElsLogPath);

        log.ValidElsVcfFiles = new List<string>();
        log.InvalidElsVcfFiles = new List<string>();

        var regexElsVersion = new Regex($"VER\\s+\\[ ([0-9.]+) \\]");
        var matchElsVersion = regexElsVersion.Match(wholeLog);
        if (matchElsVersion.Success)
        {
            log.ElsVersion = matchElsVersion.Groups[1].Value;
        }

        var regexAHV = new Regex($"\\s+\\(OK\\) AdvancedHookV\\.dll file found and successfully initialized\\.");
        log.AdvancedHookVFound = regexAHV.Match(wholeLog).Success;

        var regexVcfContainer = new Regex($"<VCONT> Attempting to open VCF container at: \\[ \\.(.*) \\]\\.\\r\\n\\s+\\(OK\\) VCF container found, scanning for files\\.", RegexOptions.Multiline);
        var matchVcfContainer = regexVcfContainer.Match(wholeLog);
        if (matchVcfContainer.Success) 
        {
            log.VcfContainer = matchVcfContainer.Groups[1].Value.Replace("//", "/");
            Console.WriteLine($"ELS VCF Container is located here: {log.VcfContainer}");
        }

        var regexValidVcfFile = new Regex($"<FILEV> Found file: \\[ (.*) \\]\\.\\r\\n<FILEV> Verifying vehicle model name \\[ .* \\]\\.\\r\\n\\s+\\(OK\\) Is valid vehicle model, assigned VMID \\[ .* \\]\\.\\r\\n\\s+Parsing file. \\*A crash before all clear indicates faulty VCF\\.\\*\\r\\n\\s+VCF Description: .*\\r\\n\\s+VCF Author: .*(\\r\\n\\s+\\(OK\\) Collected data from: '\\w+'\\.)+\\r\\n\\s+\\(OK\\) ALL CLEAR -- Configuration file processed\\.");
        var matchesValidVcfFile = regexValidVcfFile.Matches(wholeLog);
        foreach (Match match in matchesValidVcfFile)
        {
            log.ValidElsVcfFiles.Add(match.Groups[1].Value);
        }

        var regexInvalidVcfFile = new Regex($"<FILEV> Found file: \\[ (.*) \\]\\.\\r\\n<FILEV> Verifying vehicle model name \\[ .* \\]\\.\\r\\n\\s+\\(ER\\) Model specified is not valid, discarding file\\.");
        var matchesInvalidVcfFile = regexInvalidVcfFile.Matches(wholeLog);
        foreach (Match match in matchesInvalidVcfFile)
        {
            log.InvalidElsVcfFiles.Add(match.Groups[1].Value);
        }

        var regexFaultyVcf = new Regex($"\\s+\\(OK\\) Collected data from: '\\w+'\\.\\r\\n$");
        var matchFaultyVcf = regexFaultyVcf.Match(wholeLog);
        if (matchFaultyVcf.Success)
        {
            var regexElsXml = new Regex("<FILEV> Found file: \\[ (.+\\.xml) \\]\\.");
            var matchesElsXml = regexElsXml.Matches(wholeLog);

            log.FaultyVcfFile = matchesElsXml[matchesElsXml.Count - 1].Groups[1].Value;
        }

        var regexTotalAmountVcfs = new Regex($"<FILEV> Vehicle information retrieved\\. Loaded \\[ (\\d+) \\] ELS-enabled model\\(s\\)\\.");
        var matchTotalAmountVcfs = regexTotalAmountVcfs.Match(wholeLog);
        if (matchTotalAmountVcfs.Success) 
        {
            log.TotalAmountElsModels = int.Parse(matchTotalAmountVcfs.Groups[1].Value);
        }
        else {
            log.TotalAmountElsModels = null;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Log Processed...");
        Console.WriteLine("");
        Console.WriteLine($"The ELS VCF (XML) file that caused the crash is {log.FaultyVcfFile}");
        Console.WriteLine($"Valid ELS XML Files: {log.ValidElsVcfFiles.Count}");
        Console.WriteLine($"Invalid ELS XML Files: {log.InvalidElsVcfFiles.Count}");
        Console.WriteLine($"Total amount of ELS-enabled models: {log.TotalAmountElsModels}");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.White;
        return log;
    }
}