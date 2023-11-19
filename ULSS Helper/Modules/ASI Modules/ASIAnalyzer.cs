using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ASI_Modules;

// ReSharper disable InconsistentNaming
public class ASIAnalyzer
{
    internal static ASILog Run(string attachmentUrl)
    {
        var timer = new Stopwatch();
        timer.Start();
#pragma warning disable SYSLIB0014
        using var client = new WebClient();
        string fullFilePath = Settings.GenerateNewFilePath(FileType.ASI_LOG);
        client.DownloadFile(attachmentUrl, fullFilePath);

        // ReSharper disable once UseObjectOrCollectionInitializer
        var log = new ASILog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = File.ReadAllText(path: fullFilePath);
        log.LoadedAsiFiles = new List<string>();
        log.FailedAsiFiles = new List<string>();
        
        var regexFailedAsi = new Regex(@"^\s+.(.+.asi). failed to load.*", RegexOptions.Multiline);
        var matchesFailedAsi = regexFailedAsi.Matches(wholeLog);
        foreach (Match match in matchesFailedAsi)
        {
            log.FailedAsiFiles.Add(match.Groups[1].Value);
        }
        
        var regexLoadedAsi = new Regex(@"^\s+.(.+.asi). (?!failed to load).*", RegexOptions.Multiline);
        var matchesLoadedAsi = regexLoadedAsi.Matches(wholeLog);
        foreach (Match match in matchesLoadedAsi)
        {
            log.LoadedAsiFiles.Add(match.Groups[1].Value);
        }
        
        timer.Stop();
        log.ElapsedTime = timer.ElapsedMilliseconds.ToString();
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ASI Log Processed...");
        Console.WriteLine($"Time: {log.ElapsedTime}MS");
        Console.WriteLine("");
        Console.WriteLine($"Loaded: {log.LoadedAsiFiles.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Failed: {log.FailedAsiFiles.Count}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        return log;
    }
}