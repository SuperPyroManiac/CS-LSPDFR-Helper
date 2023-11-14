using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.SHVDN_Modules;

public class SHVDNAnalyzer
{
    internal static SHVDNLog Run(string attachmentUrl)
    {
        using var client = new WebClient();
        client.DownloadFile(
            attachmentUrl,
            Path.Combine(
                Directory.GetCurrentDirectory(), 
                "SHVDNLogs", 
                Settings.ShvdnLogNamer()
            )
        );

        var log = new SHVDNLog();
        var wholeLog = File.ReadAllText(Settings.ShvdnLogPath);
        log.Scripts = new List<string>();
        log.MissingDepends = new List<string>();
        
        var missingDependsShvdn = new Regex(@".+\[ERROR\] .+ (.+\.dll): System\.IO\.FileNotFoundException: (\w+\s)+\W(?!RagePluginHook)(.+), Version=.+, Culture=.+PublicKeyToken=.+");
        var matchesMissingShvdn = missingDependsShvdn.Matches(wholeLog);
        foreach (Match match in matchesMissingShvdn)
        {
            log.Scripts.Add(match.Groups[1].Value);
            log.MissingDepends.Add(match.Groups[3].Value);
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SHVDN Log Processed...");
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