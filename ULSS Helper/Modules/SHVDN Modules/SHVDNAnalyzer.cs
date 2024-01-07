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
        log.Scripts = [];
        log.MissingDepends = [];
        
        var missingDependsShvdn = new Regex(@".+\[ERROR\] .+ (.+\.dll): System\.IO\.FileNotFoundException: (\w+\s)+\W(?!RagePluginHook)(.+), Version=.+, Culture=.+PublicKeyToken=.+");
        var matchesMissingShvdn = missingDependsShvdn.Matches(wholeLog);
        foreach (Match match in matchesMissingShvdn)
        {
            log.Scripts.Add(match.Groups[1].Value);
            log.MissingDepends.Add(match.Groups[3].Value);
        }
        
        timer.Stop();
        log.ElapsedTime = timer.ElapsedMilliseconds.ToString();
        log.AnalysisCompletedAt = DateTime.Now;
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SHVDN Log Processed...");
        Console.WriteLine($"Time: {log.ElapsedTime}MS");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Missing: {log.MissingDepends.Count}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        return log;
    }
}