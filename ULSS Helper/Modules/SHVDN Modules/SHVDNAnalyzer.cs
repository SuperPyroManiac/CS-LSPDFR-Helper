using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.SHVDN_Modules;

// ReSharper disable once InconsistentNaming
public class SHVDNAnalyzer
{
    internal static async Task<SHVDNLog> Run(string attachmentUrl)
    {
        var timer = new Stopwatch();
        timer.Start();
        var log = new SHVDNLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        log.FrozenScripts = [];
        log.ScriptDepends = [];

        var missingDepends = new Regex(@"Could not load file or assembly \W?(.+)(?<!RagePluginHook), Version=.+");
        var matchesMissing = missingDepends.Matches(wholeLog);
        var frozenScripts = new Regex(@"file name: (.+)\.dll\) was terminated because it caused the game to freeze");
        var matchedFs = frozenScripts.Matches(wholeLog);
        var missingFiles = new Regex(@"Could not find file \W?.+(?:Grand Theft Auto V\\|GTAV\\)([a-zA-Z.\\/0-9]+)");
        var matchedMf = missingFiles.Matches(wholeLog);
        
        foreach (Match match in matchesMissing)
        {
            if (!log.ScriptDepends.Contains(match.Groups[1].Value))
            {
                log.ScriptDepends.Add(match.Groups[1].Value); 
            }
        }
        
        foreach (Match match in matchedFs)
        {
            if (!log.FrozenScripts.Contains(match.Groups[1].Value))
            {
                log.FrozenScripts.Add(match.Groups[1].Value);
            } 
        }
        
        foreach (Match match in matchedMf)
        {
            if (!log.ScriptDepends.Contains(match.Groups[1].Value))
            {
                log.ScriptDepends.Add(match.Groups[1].Value);
            }
        }

        timer.Stop();
        log.ElapsedTime = timer.ElapsedMilliseconds.ToString();
        log.AnalysisCompletedAt = DateTime.Now;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SHVDN Log Processed...");
        Console.WriteLine($"Time: {log.ElapsedTime}MS");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Missing: {log.ScriptDepends.Count}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        return log;
    }
}