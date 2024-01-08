using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;

public class RPHAnalyzer
{
    internal static async Task<RPHLog> Run(string attachmentUrl)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        var pluginData = Database.LoadPlugins();
        var errorData = Database.LoadErrors();
        var log = new RPHLog();
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        var reader = wholeLog.Split("\r\n");

        List<Plugin> unsorted = [];
        log.RPHPlugin = [];
        log.Current = [];
        log.Outdated = [];
        log.Broken = [];
        log.Library = [];
        log.Missing = [];
        log.Missmatch = [];
        log.Errors = [];
        log.MissingDepend = [];
        log.IncorrectScripts = [];
        log.IncorrectPlugins = [];
        log.IncorrectLibs = [];
        log.IncorrectOther = [];
        log.DownloadLink = attachmentUrl;

        if (reader.Length > 0)
            log.FilePossiblyOutdated = IsPossiblyOutdatedFile(reader[0]);

        foreach (var lineReader in reader)
        {
            var line = lineReader;
            var allrounder = new Regex(@".+LSPD First Response: (\W*\w*\W*\w*\W*), Version=([0-9]+\..+), Culture=\w+, PublicKeyToken=\w+");
            var allmatch = allrounder.Match(line);
            if (allmatch.Success)
            {
                line = line.Substring(line.LastIndexOf(": "));
                line = line.Replace(": ", string.Empty);
                line = line.Substring(0, line.IndexOf(", ") + 1);
                line = line.Replace(",", string.Empty);
                if (line.Length > 1 && unsorted.All(x => x.Name != line))
                {
                    var temp = new Plugin();
                    temp.Name = line;
                    temp.Version = allmatch.Groups[2].Value;
                    unsorted.Add(temp);
                }
            }
            
            var rphFinder = new Regex(@"Loading plugin .+\wlugins(?:\\|/)(.+).dll.*");
            var rphMatch = rphFinder.Match(line);
            if (rphMatch.Success)
            {
                var rphPlug = new Plugin();
                rphPlug.Name = rphMatch.Groups[1].Value;
                if (log.RPHPlugin.All(x => x.Name != rphPlug.Name)) log.RPHPlugin.Add(rphPlug);
            }
            
            var rphver = new Regex(@".+ Version: RAGE Plugin Hook v(\d+\.\d+\.\d+\.\d+) for Grand Theft Auto V");
            var match1 = rphver.Match(line);
            if (match1.Success) log.RPHVersion = match1.Groups[1].Value;
            
            var gtaver = new Regex(@".+ Product version: (\d+\.\d+\.\d+\.\d+)");
            var match2 = gtaver.Match(line);
            if (match2.Success) log.GTAVersion = match2.Groups[1].Value;
            
            var lspdfrver = new Regex(@".+ Running LSPD First Response 0\.4\.9 \((\d+\.\d+\.\d+\.\d+)\)");
            var match3 = lspdfrver.Match(line);
            if (match3.Success) log.LSPDFRVersion = match3.Groups[1].Value;
            
            if (lineReader.Contains("LSPD First Response: Creating plugin")) break;
        }

        foreach (var plugin in pluginData)
        {
            try
            {
                if (unsorted.All(x => x.Name != plugin.Name)) continue;
                var logPlug = unsorted.First(x => x.Name == plugin.Name);
                switch (plugin.State)
                {
                    case "LSPDFR" or "EXTERNAL":
                    {
                        if (logPlug.Name == plugin.Name)
                        {
                            //Check EA Version
                            if (!string.IsNullOrEmpty(plugin.EAVersion) && logPlug.Version == plugin.EAVersion && log.Current.All(x => x.Name != plugin.Name))
                            {
                                log.Current.Add(plugin);
                                unsorted.Remove(logPlug);
                                break;
                            }

                            //Compare Versions
                            if (!string.IsNullOrEmpty(plugin.Version))
                            {
                                var result = CompareVersions(logPlug.Version, plugin.Version);
                                switch (result)
                                {
                                    // plugin version in log is older than version in DB
                                    case < 0:
                                    {
                                        if (log.Outdated.All(x => x.Name != plugin.Name)) log.Outdated.Add(plugin);
                                        unsorted.Remove(logPlug);
                                        break;
                                    }
                                    // plugin version in log is newer than version in DB and there is no Early Acccess version
                                    case > 0:
                                    {
                                        plugin.EAVersion = logPlug.Version; // save logVersion in log.Missmatch so we can access it later when building bot responses
                                        if (log.Missmatch.All(x => x.Name != plugin.Name)) log.Missmatch.Add(plugin);
                                        unsorted.Remove(logPlug);
                                        break;
                                    }
                                    // plugin version in log is up to date (equals plugin version number in DB)
                                    default:
                                    {
                                        if (log.Current.All(x => x.Name != plugin.Name)) log.Current.Add(plugin);
                                        unsorted.Remove(logPlug);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    case "BROKEN":
                    {
                        if (logPlug.Name == plugin.Name)
                        {
                            if (log.Broken.All(x => x.Name != plugin.Name)) log.Broken.Add(plugin);
                            unsorted.Remove(logPlug);
                        }
                        break;
                    }
                    case "LIB":
                    {
                        if (logPlug.Name == plugin.Name)
                        {
                            if (log.Library.All(x => x.Name != plugin.Name)) log.Library.Add(plugin);
                            unsorted.Remove(logPlug);
                        }
                        break;
                    }
                    case "IGNORE":
                    {
                        if (logPlug.Name == plugin.Name)
                        {
                            if (log.Current.All(x => x.Name != plugin.Name)) log.Current.Add(plugin);
                            unsorted.Remove(logPlug);
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
        log.Missing.AddRange(unsorted);

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
        Console.WriteLine($"Errors: {log.Errors.Count}");
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
        var parts1 = version1.Split('.');
        var parts2 = version2.Split('.');
        
        var minLength = Math.Min(parts1.Length, parts2.Length);

        for (var i = 0; i < minLength; i++)
        {
            var part1 = int.Parse(parts1[i]);
            var part2 = int.Parse(parts2[i]);

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
        var dateLineRegex = new Regex(@".+Started new log on \D*(\d+\W{1,2}\d+\W{1,2}\d+\S{0,1}|\d+\W[a-zA-Z]{3}\W\d+)\D*(\d{1,2}\W\d{1,2}\W\d{1,2})\s*\D*\.\d{1,3}");
        var dateLineMatch = dateLineRegex.Match(dateLine);
        if (!dateLineMatch.Success) 
            return false;
        
        var dateString = dateLineMatch.Groups[1].Value; 
        var timeString = dateLineMatch.Groups[2].Value;
        var dateTimeString = dateString + " " + timeString;

        var dateRegex1 = new Regex(@"(\d+)(\W{1,2})(\d+)(\W{1,2})(\d+)(\S{0,1})");
        var dateRegex2 = new Regex(@"(\d+)(\W)([a-zA-Z]{3})(\W)(\d+)");
        var timeRegex = new Regex(@"(\d{1,2})(\W)(\d{1,2})(\W)(\d{1,2})");

        var dateMatch1 = dateRegex1.Match(dateString);
        var dateMatch2 = dateRegex2.Match(dateString);
        var timeMatch = timeRegex.Match(timeString);

        var timeSep1 = ":";
        var timeSep2 = ":";
        if (timeMatch.Success)
        {
            timeSep1 = timeMatch.Groups[2].Value;
            timeSep2 = timeMatch.Groups[4].Value;
        }

        List<string> dateFormats = [];
        if (dateMatch1.Success)
        {
            var sep1 = dateMatch1.Groups[2].Value ?? "";
            var sep2 = dateMatch1.Groups[4].Value ?? "";
            var sep3 = dateMatch1.Groups[6].Value ?? "";
            dateFormats.Add($"d{sep1}M{sep2}yyyy{sep3} H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"M{sep1}d{sep2}yyyy{sep3} H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"yyyy{sep1}M{sep2}d{sep3} H{timeSep1}mm{timeSep2}ss");
        }
        else if (dateMatch2.Success)
        {
            var sep1 = dateMatch2.Groups[2].Value ?? "";
            var sep2 = dateMatch2.Groups[4].Value ?? "";
            dateFormats.Add($"d{sep1}MMM{sep2}yyyy H{timeSep1}mm{timeSep2}ss");
            dateFormats.Add($"yyyy{sep1}MMM{sep2}d H{timeSep1}mm{timeSep2}ss");
        }

        List<DateTime> parsedDates = [];

        var success = DateTime.TryParse(dateTimeString, out var parsedDate1);
        if (success)
            parsedDates.Add(parsedDate1);

        foreach (var dateFormat in dateFormats)
        {
            var successExact = DateTime.TryParseExact(dateTimeString, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate2);
            if (successExact)
                parsedDates.Add(parsedDate2);
        }        
        
        var currentDate = DateTime.Now;
        var currentDateWithBuffer = currentDate.AddHours(24); // add a buffer of 24h to allow for any time zone differences
        var closestDate = DateTime.MinValue;
        var closestDifference = TimeSpan.MaxValue;
        var noValidResult = true;

        /*
        The following loop determines the date in the list of parsedDates that...
        - is not more than 24h in the future (doesn't make sense)
        - is the closest to the currentDate (because we don't know the actual correct date format and want to assume that the correct date is the most recent one)

        If no parsedDate meets the two conditions above, we can't say anything meaningful about the age of the log file (noValidResult remains true)
        */
        foreach (var parsedDate in parsedDates)
        {
            if (parsedDate <= currentDateWithBuffer)
            {
                var difference = currentDate - parsedDate;
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

        var difference2 = currentDateWithBuffer - closestDate;
        if (difference2.TotalHours > 48)
            return true; // the uploaded RPH log is older than 24h compared to the current dateTime (excluding the 24h buffer to allow time zone differences)

        return false;
    }
}