using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ASI_Modules;

public class ASIAnalyzer
{
    internal static ASILog Run(string attachmentUrl)
    {
        using var client = new WebClient();
        string fullFilePath = Settings.GenerateNewFilePath(FileType.ASI_LOG);
        client.DownloadFile(attachmentUrl, fullFilePath);

        var log = new ASILog();
        var wholeLog = File.ReadAllText(path: fullFilePath);
        log.LoadedASIFiles = new List<string>();
        log.FailedASIFiles = new List<string>();
        
        var regexFailedASI = new Regex(@"^\s+.(.+.asi). failed to load.*", RegexOptions.Multiline);
        var matchesFailedASI = regexFailedASI.Matches(wholeLog);
        foreach (Match match in matchesFailedASI)
        {
            log.FailedASIFiles.Add(match.Groups[1].Value);
        }
        
        var regexLoadedASI = new Regex(@"^\s+.(.+.asi). (?!failed to load).*", RegexOptions.Multiline);
        var matchesLoadedASI = regexLoadedASI.Matches(wholeLog);
        foreach (Match match in matchesLoadedASI)
        {
            log.LoadedASIFiles.Add(match.Groups[1].Value);
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ASI Log Processed...");
        Console.WriteLine("");
        Console.WriteLine($"Loaded: {log.LoadedASIFiles.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed: {log.FailedASIFiles.Count}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        return log;
    }
}