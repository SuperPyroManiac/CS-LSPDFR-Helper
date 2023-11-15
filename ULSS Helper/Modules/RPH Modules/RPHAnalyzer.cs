using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;

public class RPHAnalyzer
{
    internal static RPHLog Run(string attachmentUrl)
    {
        using var client = new WebClient();
        string fullFilePath = Settings.GenerateNewFilePath(FileType.RPH_LOG);
        client.DownloadFile(attachmentUrl, fullFilePath);

        var pluginData = Database.LoadPlugins();
        var errorData = Database.LoadErrors();
        var log = new RPHLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = File.ReadAllText(fullFilePath);
        var reader = File.ReadAllLines(fullFilePath);

        log.Current = new List<Plugin?>();
        log.Outdated = new List<Plugin?>();
        log.Broken = new List<Plugin?>();
        log.Library = new List<Plugin?>();
        log.Missing = new List<Plugin?>();
        log.Missmatch = new List<Plugin>();
        log.Errors = new List<Error?>();

        if (reader.Length > 0)
            log.FilePossiblyOutdated = IsPossiblyOutdatedFile(reader[0]);

        foreach (var lineReader in reader)
        {
            var line = lineReader;
            foreach (var plugin in pluginData)
            {
                try
                {
                    switch (plugin.State)
                    {
                        case "LSPDFR" or "EXTERNAL":
                        {
                            var regex = new Regex($".+LSPD First Response: {Regex.Escape(plugin.Name)}. Version=([0-9.]+).+");
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                string logVersion = match.Groups[1].Value;
                                var version = $"{plugin.Name}, Version={plugin.Version}";
                                var eaversion = $"{plugin.Name}, Version={plugin.EAVersion}";
                                if (!string.IsNullOrEmpty(plugin.Version))
                                {
                                    var result = CompareVersions(logVersion, plugin.Version);
                                    if (result < 0) // plugin version in log is older than version in DB
                                    {
                                        if (!log.Outdated.Any(x => x.Name == plugin.Name)) log.Outdated.Add(plugin);
                                    }
                                    else if (result > 0) // plugin version in log is newer than version in DB
                                    {
                                        if (!string.IsNullOrEmpty(plugin.EAVersion)) 
                                        {
                                            var resultEA = CompareVersions(logVersion, plugin.EAVersion);
                                            if (resultEA < 0) // plugin version in log is older than Early Access version in DB
                                            {
                                                if (!log.Outdated.Any(x => x.Name == plugin.Name)) log.Outdated.Add(plugin);
                                            }
                                            else if (resultEA > 0) // plugin version in log is newer than Early Access version in DB
                                            {
                                                plugin.EAVersion = logVersion; // save logVersion in log.Missmatch so we can access it later when building bot responses
                                                if (!log.Missmatch.Any(x => x.Name == plugin.Name)) log.Missmatch.Add(plugin);
                                            }
                                            else // plugin version in log is up to date (equals Early Access version in DB)
                                            {
                                                if (!log.Current.Any(x => x.Name == plugin.Name)) log.Current.Add(plugin);
                                            }
                                        } 
                                        else // plugin version in log is newer than version in DB and there is no Early Acccess version
                                        {
                                            plugin.EAVersion = logVersion; // save logVersion in log.Missmatch so we can access it later when building bot responses
                                            if (!log.Missmatch.Any(x => x.Name == plugin.Name)) log.Missmatch.Add(plugin);
                                        }
                                    }
                                    else // plugin version in log is up to date (equals plugin version number in DB)
                                    {
                                        if (!log.Current.Any(x => x.Name == plugin.Name)) log.Current.Add(plugin);
                                    }
                                }
                            }
                            break;
                        }
                        case "BROKEN":
                        {
                            var regex = new Regex($".+LSPD First Response: {Regex.Escape(plugin.Name)}. Version=[0-9.]+.+");
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                if (!log.Broken.Any(x => x.Name == plugin.Name)) log.Broken.Add(plugin);
                            }
                            break;
                        }
                        case "LIB":
                        {
                            var regex = new Regex($".+LSPD First Response: {Regex.Escape(plugin.Name)}. Version=[0-9.]+.+");
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                if (!log.Library.Any(x => x.Name == plugin.Name)) log.Library.Add(plugin);
                            }
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.ErrLog(e.ToString());
                    Console.WriteLine(e);
                    throw;
                }
            }
            
            var allrounder = new Regex(".+LSPD First Response: (\\W*\\w*\\W*\\w*\\W*), Version=([0-9]+\\..+), Culture=\\w+, PublicKeyToken=\\w+");
            var allmatch = allrounder.Match(line);
            if (allmatch.Success)
            {
                line = line.Substring(line.LastIndexOf(": "));
                line = line.Replace(": ", string.Empty);
                line = line.Substring(0, line.IndexOf(", ") + 1);
                line = line.Replace(",", string.Empty);
                if (line.Length > 1 && !log.Current.Any(x => x.Name == line) &&
                    !log.Outdated.Any(x => x.Name == line) && !log.Broken.Any(x => x.Name == line) &&
                    !log.Library.Any(x => x.Name == line) && !log.Missing.Any(x => x.Name == line) && 
                    !log.Missmatch.Any(x => x.Name == line))
                {
                    // save info from log about unrecognized plugin in log.Missing so we can access it later when building bot responses
                    var temp = new Plugin();
                    temp.Name = line;
                    temp.Version = allmatch.Groups[2].Value;
                    temp.State = "MISSING";
                    log.Missing.Add(temp);
                }
            }
            
            var rphver = new Regex(@".+ Version: RAGE Plugin Hook v(\d+\.\d+\.\d+\.\d+) for Grand Theft Auto V");
            Match match1 = rphver.Match(line);
            if (match1.Success) log.RPHVersion = match1.Groups[1].Value;
            
            var gtaver = new Regex(@".+ Product version: (\d+\.\d+\.\d+\.\d+)");
            Match match2 = gtaver.Match(line);
            if (match2.Success) log.GTAVersion = match2.Groups[1].Value;
            
            var lspdfrver = new Regex(@".+ Running LSPD First Response 0\.4\.9 \((\d+\.\d+\.\d+\.\d+)\)");
            Match match3 = lspdfrver.Match(line);
            if (match3.Success) log.LSPDFRVersion = match3.Groups[1].Value;
        }

        foreach (var error in errorData)
        {
            var errregex = new Regex(error.Regex);
            var errmatch = errregex.Matches(wholeLog);
            foreach (Match match in errmatch)
            {
                var newError = new Error()
                { ID = error.ID, Level = error.Level, Regex = error.Regex, Solution = error.Solution };
                for (var i = 0; i <= 10; i++)
                {
                    newError.Solution = newError.Solution.Replace("{" + i + "}", match.Groups[i].Value);
                }
                if (!log.Errors.Any(x => x.Solution == newError.Solution)) log.Errors.Add(newError);
            }
        }
        log.Errors = log.Errors.OrderBy(x => x.Level).ToList();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("RPH Log Processed...");
        Console.WriteLine("");
        Console.WriteLine($"Current: {log.Current.Count}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Outdated: {log.Outdated.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Broken: {log.Broken.Count}");
        Console.WriteLine($"Library: {log.Library.Count}");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Missing: {log.Missing.Count}");
        Console.WriteLine($"Newer: {log.Missmatch.Count}");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.White;
        return log;
    }
    
    public static int CompareVersions(string version1, string version2)
    {
        string[] parts1 = version1.Split('.');
        string[] parts2 = version2.Split('.');
        
        int minLength = Math.Min(parts1.Length, parts2.Length);

        for (int i = 0; i < minLength; i++)
        {
            int part1 = int.Parse(parts1[i]);
            int part2 = int.Parse(parts2[i]);

            if (part1 < part2)
            {
                return -1; // version1 is smaller
            }
            else if (part1 > part2)
            {
                return 1; // version1 is larger
            }
        }

        // If all common parts are equal, check the remaining parts
        if (parts1.Length < parts2.Length)
        {
            return -1; // version1 is smaller
        }
        else if (parts1.Length > parts2.Length)
        {
            return 1; // version1 is larger
        }
        
        return 0; // versions are equal
    }

    private static bool IsPossiblyOutdatedFile(string dateLine)
    {
        Regex? dateLineRegex = new Regex(@".+Started new log on \D*(\d+\W{1,2}\d+\W{1,2}\d+\S{0,1}|\d+\W\w+\W\d+)\D*(\d{1,2}\W\d{1,2}\W\d{1,2})\s*\D*\.\d{1,3}");
        Match? dateLineMatch = dateLineRegex.Match(dateLine);
        string dateString = dateLineMatch.Groups[1].Value; 
        string timeString = dateLineMatch.Groups[2].Value;
        string dateTimeString = dateString + " " + timeString;

        if (!dateLineMatch.Success) return false;

        Regex dateRegex1 = new Regex(@"(\d+)(\W{1,2})(\d+)(\W{1,2})(\d+)(\S{0,1})");
        Regex dateRegex2 = new Regex(@"(\d+)(\W)(\w+)(\W)(\d+)");
        Regex timeRegex = new Regex(@"(\d{1,2})(\W)(\d{1,2})(\W)(\d{1,2})");

        Match dateMatch1 = dateRegex1.Match(dateString);
        Match dateMatch2 = dateRegex2.Match(dateString);
        Match timeMatch = timeRegex.Match(timeString);

        string timeSep1 = ":";
        string timeSep2 = ":";
        if (timeMatch.Success)
        {
            timeSep1 = timeMatch.Groups[2].Value;
            timeSep2 = timeMatch.Groups[4].Value;
        }

        List<string> genericFormats = new();
        if (dateMatch1.Success)
        {
            string sep1 = dateMatch1.Groups[2].Value;
            string sep2 = dateMatch1.Groups[4].Value;
            string sep3 = dateMatch1.Groups[6].Value;
            genericFormats.Add($"dd{sep1}MM{sep2}yyyy{sep3} HH{timeSep1}mm{timeSep2}ss");
            genericFormats.Add($"MM{sep1}dd{sep2}yyyy{sep3} HH{timeSep1}mm{timeSep2}ss");
            genericFormats.Add($"yyyy{sep1}MM{sep2}dd{sep3} HH{timeSep1}mm{timeSep2}ss");
        }
        else if (dateMatch2.Success)
        {
            string sep1 = dateMatch2.Groups[2].Value;
            string sep2 = dateMatch2.Groups[4].Value;
            genericFormats.Add($"dd{sep1}MMM{sep2}yyyy HH{timeSep1}mm{timeSep2}ss");
            genericFormats.Add($"yyyy{sep1}MMM{sep2}dd HH{timeSep1}mm{timeSep2}ss");
        }

        List<DateTime> results = new();
        DateTime parsedDate1;

        if (DateTime.TryParse(dateTimeString, out parsedDate1))
            results.Add(parsedDate1);

        foreach (string genericFormat in genericFormats)
        {
            if (DateTime.TryParseExact(dateTimeString, genericFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate2))
                results.Add(parsedDate2);
        }        
        
        DateTime currentDate = DateTime.Now;
        DateTime closestDate = DateTime.MinValue;
        TimeSpan closestDifference = TimeSpan.MaxValue;

        foreach (DateTime result in results)
        {
            TimeSpan difference = result - currentDate;
            
            if (difference < closestDifference)
            {
                closestDifference = difference;
                closestDate = result;
            }
        }

        TimeSpan difference2 = currentDate - closestDate;
        if (difference2.TotalHours > 24) 
            return true;

        return false;
    }
}