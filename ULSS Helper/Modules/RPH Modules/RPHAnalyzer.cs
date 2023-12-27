using System.Globalization;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;

public class RPHAnalyzer
{
    internal static RPHLog Run(string attachmentUrl)
    {
        var timer = new Stopwatch();
        timer.Start();
#pragma warning disable SYSLIB0014
        using var client = new WebClient();
        string fullFilePath = Settings.GenerateNewFilePath(FileType.RPH_LOG);
        client.DownloadFile(attachmentUrl, fullFilePath);

        var pluginData = Database.LoadPlugins();
        var errorData = Database.LoadErrors();
        var log = new RPHLog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = File.ReadAllText(fullFilePath);
        var reader = File.ReadAllLines(fullFilePath);

        log.RPHPlugin = new List<Plugin>();
        log.Current = new List<Plugin>();
        log.Outdated = new List<Plugin>();
        log.Broken = new List<Plugin>();
        log.Library = new List<Plugin>();
        log.Missing = new List<Plugin>();
        log.Missmatch = new List<Plugin>();
        log.Errors = new List<Error>();
        log.MissingDepend = new List<Plugin>();
        log.IncorrectScripts = new List<string>();
        log.IncorrectPlugins = new List<string>();
        log.IncorrectLibs = new List<string>();
        log.IncorrectOther = new List<string>();

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
                                var logVersion = match.Groups[1].Value;

                                //Check EA Version
                                if (!string.IsNullOrEmpty(plugin.EAVersion) && logVersion == plugin.EAVersion && log.Current.All(x => x.Name != plugin.Name))
                                {
                                    log.Current.Add(plugin);
                                    break;
                                }

                                //Compare Versions
                                if (!string.IsNullOrEmpty(plugin.Version))
                                {
                                    var result = CompareVersions(logVersion, plugin.Version);
                                    switch (result)
                                    {
                                        // plugin version in log is older than version in DB
                                        case < 0:
                                        {
                                            if (log.Outdated.All(x => x.Name != plugin.Name)) log.Outdated.Add(plugin);
                                            break;
                                        }
                                        // plugin version in log is newer than version in DB and there is no Early Acccess version
                                        case > 0:
                                        {
                                            plugin.EAVersion = logVersion; // save logVersion in log.Missmatch so we can access it later when building bot responses
                                            if (log.Missmatch.All(x => x.Name != plugin.Name)) log.Missmatch.Add(plugin);
                                            break;
                                        }
                                        // plugin version in log is up to date (equals plugin version number in DB)
                                        default:
                                        {
                                            if (log.Current.All(x => x.Name != plugin.Name)) log.Current.Add(plugin);
                                            break;
                                        }
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
                                if (log.Broken.All(x => x.Name != plugin.Name)) log.Broken.Add(plugin);
                            }
                            break;
                        }
                        case "LIB":
                        {
                            var regex = new Regex($".+LSPD First Response: {Regex.Escape(plugin.Name)}. Version=[0-9.]+.+");
                            var match = regex.Match(line);
                            if (match.Success)
                            {
                                if (log.Library.All(x => x.Name != plugin.Name)) log.Library.Add(plugin);
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
            
            var allrounder = new Regex(@".+LSPD First Response: (\W*\w*\W*\w*\W*), Version=([0-9]+\..+), Culture=\w+, PublicKeyToken=\w+");
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
            
            var rphFinder = new Regex(@"Plugin \W?.+\W? was loaded from \W?(.+\.dll)\W?");
            var rphMatch = rphFinder.Match(line);
            if (rphMatch.Success)
            {
                var rphPlug = new Plugin();
                rphPlug.Name = rphMatch.Groups[1].Value;
                if (log.RPHPlugin.All(x => x.Name != rphPlug.Name)) log.RPHPlugin.Add(rphPlug);
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
            if (error.ID is "1" or "97" or "98" or "99" or "41") continue;
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

        var dependmatch = new Regex(errorData[0].Regex).Matches(wholeLog);
        foreach (Match match in dependmatch)
        {
            if (log.MissingDepend.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            var newDepend = new Plugin { Name = match.Groups[2].Value, DName = match.Groups[2].Value};
            foreach (var plugin in pluginData.Where(plugin => plugin.Name.Equals(newDepend.Name)))
            {newDepend.DName = plugin.DName; newDepend.Link = plugin.Link; log.MissingDepend.Add(newDepend);}
        }
        if (log.MissingDepend.Count != 0)
        {
            var linkedDepend = log.MissingDepend.Select(
	            plugin => plugin?.Link != null && plugin.Link.StartsWith("https://")
		            ? $"[{plugin.DName}]({plugin.Link})"
		            : $"[{plugin?.DName}](https://www.google.com/search?q=lspdfr+{plugin.Name.Replace(" ", "+")})"
            ).ToList();
            var linkedDependstring = string.Join("\r\n- ", linkedDepend);
            var dependErr = errorData[0];
            dependErr.Solution = $"{errorData[0].Solution}\r\n- {linkedDependstring}";
            if (dependErr.Solution.Length >= 1024) dependErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(dependErr);
        }
        var libErr = errorData.Find(x => x.ID == "97");
        var libssmatch = new Regex(libErr.Regex).Matches(wholeLog);
        foreach (Match match in libssmatch)
        {
            if (log.IncorrectLibs.Any(x => x.Equals(match.Groups[1].Value))) continue;
            log.IncorrectLibs.Add(match.Groups[1].Value);
        }
        if (log.IncorrectLibs.Count != 0)
        {
            libErr.Solution = $"{libErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectLibs)}";
            if (libErr.Solution.Length >= 1024) libErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(libErr);
        }
        var scriptErr = errorData.Find(x => x.ID == "98");
        var scriptsmatch = new Regex(scriptErr.Regex).Matches(wholeLog);
        foreach (Match match in scriptsmatch)
        {
            if (log.IncorrectScripts.Any(x => x.Equals(match.Groups[1].Value))) continue;
            log.IncorrectScripts.Add(match.Groups[1].Value);
        }
        if (log.IncorrectScripts.Count != 0)
        {
            scriptErr.Solution = $"{scriptErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectScripts)}";
            if (scriptErr.Solution.Length >= 1024) scriptErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(scriptErr);
        }
        var plugErr = errorData.Find(x => x.ID == "99");
        var plugssmatch = new Regex(plugErr.Regex).Matches(wholeLog);
        foreach (Match match in plugssmatch)
        {
            if (log.IncorrectPlugins.Any(x => x.Equals(match.Groups[1].Value))) continue;
            log.IncorrectPlugins.Add(match.Groups[1].Value);
        }
        if (log.IncorrectPlugins.Count != 0)
        {
            plugErr.Solution = $"{plugErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectPlugins)}";
            if (plugErr.Solution.Length >= 1024) plugErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(plugErr);
        }
        var plugOth = errorData.Find(x => x.ID == "41");
        var othsmatch = new Regex(plugOth.Regex).Matches(wholeLog);
        foreach (Match match in othsmatch)
        {
            if (log.IncorrectOther.Any(x => x.Equals(match.Groups[2].Value))) continue;
            log.IncorrectOther.Add(match.Groups[2].Value);
        }
        if (log.IncorrectOther.Count != 0)
        {
            plugOth.Solution = $"{plugOth.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectOther)}";
            if (plugOth.Solution.Length >= 1024) plugOth.Solution = "Too many to show! God damn!";
            log.Errors.Add(plugOth);
        }

        log.Errors = log.Errors.OrderBy(x => x.Level).ToList();
        
        timer.Stop();
        log.ElapsedTime = timer.ElapsedMilliseconds.ToString();
        log.AnalysisCompletedAt = DateTime.Now;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("RPH Log Processed...");
        Console.WriteLine($"Time: {log.ElapsedTime}MS");
        Console.WriteLine("");
        Console.WriteLine($"Current: {log.Current.Count}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Outdated: {log.Outdated.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Broken: {log.Broken.Count}");
        Console.WriteLine($"Incorrect Library: {log.Library.Count}");
        Console.WriteLine($"Missing Library: {log.MissingDepend.Count}");
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
        Regex dateLineRegex = new Regex(@".+Started new log on \D*(\d+\W{1,2}\d+\W{1,2}\d+\S{0,1}|\d+\W[a-zA-Z]{3}\W\d+)\D*(\d{1,2}\W\d{1,2}\W\d{1,2})\s*\D*\.\d{1,3}");
        Match dateLineMatch = dateLineRegex.Match(dateLine);
        if (!dateLineMatch.Success) 
            return false;
        
        string dateString = dateLineMatch.Groups[1].Value; 
        string timeString = dateLineMatch.Groups[2].Value;
        string dateTimeString = dateString + " " + timeString;

        Regex dateRegex1 = new Regex(@"(\d+)(\W{1,2})(\d+)(\W{1,2})(\d+)(\S{0,1})");
        Regex dateRegex2 = new Regex(@"(\d+)(\W)([a-zA-Z]{3})(\W)(\d+)");
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

        List<string> dateFormats = new();
        if (dateMatch1.Success)
        {
            string sep1 = dateMatch1.Groups[2].Value ?? "";
            string sep2 = dateMatch1.Groups[4].Value ?? "";
            string sep3 = dateMatch1.Groups[6].Value ?? "";
            dateFormats.Add($"d{sep1}M{sep2}yyyy{sep3} H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"M{sep1}d{sep2}yyyy{sep3} H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"yyyy{sep1}M{sep2}d{sep3} H{timeSep1}mm{timeSep2}ss");
        }
        else if (dateMatch2.Success)
        {
            string sep1 = dateMatch2.Groups[2].Value ?? "";
            string sep2 = dateMatch2.Groups[4].Value ?? "";
            dateFormats.Add($"d{sep1}MMM{sep2}yyyy H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"yyyy{sep1}MMM{sep2}d H{timeSep1}mm{timeSep2}ss");
        }

        List<DateTime> parsedDates = new();

        bool success = DateTime.TryParse(dateTimeString, out DateTime parsedDate1);
        if (success)
            parsedDates.Add(parsedDate1);

        foreach (string dateFormat in dateFormats)
        {
            bool successExact = DateTime.TryParseExact(dateTimeString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsedDate2);
            if (successExact)
                parsedDates.Add(parsedDate2);
        }        
        
        DateTime currentDate = DateTime.Now;
        DateTime currentDateWithBuffer = currentDate.AddHours(24); // add a buffer of 24h to allow for any time zone differences
        DateTime closestDate = DateTime.MinValue;
        TimeSpan closestDifference = TimeSpan.MaxValue;
        bool noValidResult = true;

        /*
        The following loop determines the date in the list of parsedDates that...
        - is not more than 24h in the future (doesn't make sense)
        - is the closest to the currentDate (because we don't know the actual correct date format and want to assume that the correct date is the most recent one)

        If no parsedDate meets the two conditions above, we can't say anything meaningful about the age of the log file (noValidResult remains true)
        */
        foreach (DateTime parsedDate in parsedDates)
        {
            if (parsedDate <= currentDateWithBuffer)
            {
                TimeSpan difference = currentDate - parsedDate;
                if (difference < closestDifference)
                {
                    closestDifference = difference;
                    closestDate = parsedDate;
                    noValidResult = false;
                }
            }
        }

        if (noValidResult) 
            return false; // we don't know whether the log file is too old, so we just assume it's the most recent log

        TimeSpan difference2 = currentDateWithBuffer - closestDate;
        if (difference2.TotalHours > 48)
            return true; // the uploaded RPH log is older than 24h compared to the current dateTime (excluding the 24h buffer to allow time zone differences)

        return false;
    }
}