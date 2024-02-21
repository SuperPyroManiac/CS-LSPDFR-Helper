using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.ASI_Modules;

// ReSharper disable InconsistentNaming
public class ASIAnalyzer
{
    internal static void BrokenASIListManager(List<string> AllASIList, List<string> loadedASI, List<string> failedASI, List<string> outdatedASI, string ASIname)
    {
        // ALLAsiList gets filled in the foreach loops before we call this method
        // Its purpose is to contain the infomration, what scripts an user is using an
        // to remove unwanted listing of scripts in the wrong section
        // Also: See Comment line 53
        if (AllASIList.Contains(ASIname))
        {
            outdatedASI.Add(ASIname);
            if (loadedASI.Contains(ASIname)) loadedASI.Remove(ASIname);
            else failedASI.Remove(ASIname);
        }
    }
    internal static async Task<ASILog> Run(string attachmentUrl)
    {
        var timer = new Stopwatch();
        timer.Start();
        var log = new ASILog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        log.LoadedAsiFiles = [];
        log.FailedAsiFiles = [];
        log.OutdatedAsiFiles = [];
        log.AllAsiFiles = [];

        var regexFailedAsi = new Regex(@"^\s+.(.+.asi). failed to load.*", RegexOptions.Multiline);
        var matchesFailedAsi = regexFailedAsi.Matches(wholeLog);
        foreach (Match match in matchesFailedAsi)
        {
            log.FailedAsiFiles.Add(match.Groups[1].Value);
            log.AllAsiFiles.Add(match.Groups[1].Value);
        }
        
        var regexLoadedAsi = new Regex(@"^\s+.(.+.asi). (?!failed to load).*", RegexOptions.Multiline);
        var matchesLoadedAsi = regexLoadedAsi.Matches(wholeLog);
        foreach (Match match in matchesLoadedAsi)
        {
            log.LoadedAsiFiles.Add(match.Groups[1].Value);
            log.AllAsiFiles.Add(match.Groups[1].Value);
        }

        #region Testing purposes. TO DO: DB Table with outdated Asi Files. Later: Iterate through DB and autofill ASIName
        // This is a WIP and its not tragic if the bot doesnt have this at the end. I just want to mess with DBS later and will take this as example
        BrokenASIListManager(log.AllAsiFiles, log.LoadedAsiFiles, log.FailedAsiFiles, log.OutdatedAsiFiles, "ScriptHookVDotNet.asi");
        BrokenASIListManager(log.AllAsiFiles, log.LoadedAsiFiles, log.FailedAsiFiles, log.OutdatedAsiFiles, "GTAV.HeapAdjuster.asi");
        #endregion

        timer.Stop();
        log.ElapsedTime = timer.ElapsedMilliseconds.ToString();
        log.AnalysisCompletedAt = DateTime.Now;
        
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