using System.Text.RegularExpressions;

namespace ULSS_Helper.Modules;

public class LogAnalyzer
{
    internal static AnalyzedLog Run()
    {
        var pluginData = DatabaseManager.LoadPlugins();
        var errorData = DatabaseManager.LoadErrors();
        var log = new AnalyzedLog();
        var wholeLog = File.ReadAllText(Settings.RphLogPath);
        var reader = new StreamReader(Settings.RphLogPath);

        log.Current = new List<Plugin?>();
        log.Outdated = new List<Plugin?>();
        log.Broken = new List<Plugin?>();
        log.Library = new List<Plugin?>();
        log.Missing = new List<Plugin?>();
        log.Errors = new List<Error?>();
        
            foreach (var plugin in pluginData)
            {
                try
                {
                    if (plugin.State is "LSPDFR" or "EXTERNAL")
                    {
                        var regex = new Regex($".+LSPD First Response: {plugin.Name}. Version=[0-9.]+.+");
                        var match = regex.Match(wholeLog);
                        if (match.Success)
                        {
                            var version = $"{plugin.Name}, Version={plugin.Version}";
                            var eaversion = $"{plugin.Name}, Version={plugin.EAVersion}";
                            if (plugin.Version != null && wholeLog.Contains(version))
                            {
                                if (!log.Current.Any(x => x.Name == plugin.Name)) log.Current.Add(plugin);
                            }
                            else if (plugin.EAVersion != null && wholeLog.Contains(eaversion))
                            {
                                if (!log.Current.Any(x => x.Name == plugin.Name)) log.Current.Add(plugin);
                            }
                            else
                            {
                                if (!log.Outdated.Any(x => x.Name == plugin.Name)) log.Outdated.Add(plugin);
                            }
                        }
                    }
                    
                    if (plugin.State == "BROKEN")
                    {
                        var regex = new Regex($".+LSPD First Response: {plugin.Name}. Version=[0-9.]+.+");
                        var match = regex.Match(wholeLog);
                        if (match.Success)
                        {
                            if (!log.Broken.Any(x => x.Name == plugin.Name)) log.Broken.Add(plugin);
                        }
                    }
                    
                    if (plugin.State == "LIB")
                    {
                        var regex = new Regex($".+LSPD First Response: {plugin.Name}. Version=[0-9.]+.+");
                        var match = regex.Match(wholeLog);
                        if (match.Success)
                        {
                            if (!log.Library.Any(x => x.Name == plugin.Name)) log.Library.Add(plugin);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            var allrounder = new Regex(".+LSPD First Response: (\\W*\\w*\\W*\\w*\\W*), Version=([0-9]+\\..+), Culture=\\w+, PublicKeyToken=\\w+");
            var allmatch = allrounder.Match(wholeLog);
            if (allmatch.Success)
            {
                wholeLog = wholeLog.Substring(wholeLog.LastIndexOf(": "));
                wholeLog = wholeLog.Replace(": ", string.Empty);
                wholeLog = wholeLog.Substring(0, wholeLog.IndexOf(", ") + 1);
                wholeLog = wholeLog.Replace(",", string.Empty);
                if (wholeLog.Length > 1 && !log.Current.Any(x => x.Name == wholeLog) &&
                    !log.Outdated.Any(x => x.Name == wholeLog) && !log.Broken.Any(x => x.Name == wholeLog) &&
                    !log.Library.Any(x => x.Name == wholeLog) && !log.Missing.Any(x => x.Name == wholeLog))
                {
                    var temp = new Plugin();
                    temp.Name = wholeLog;
                    temp.State = "MISSING";
                    log.Missing.Add(temp);
                }
            }
            
            var rphver = new Regex(@".+ Version: RAGE Plugin Hook v(\d+\.\d+\.\d+\.\d+) for Grand Theft Auto V");
            Match match1 = rphver.Match(wholeLog);
            if (match1.Success) log.RPHVersion = match1.Groups[1].Value;
            
            var gtaver = new Regex(@".+ Product version: (\d+\.\d+\.\d+\.\d+)");
            Match match2 = gtaver.Match(wholeLog);
            if (match2.Success) log.GTAVersion = match2.Groups[1].Value;
            
            var lspdfrver = new Regex(@".+ This version: (\d+\.\d+\.\d+\.\d+), Version available on server:");
            Match match3 = lspdfrver.Match(wholeLog);
            if (match3.Success) log.LSPDFRVersion = match3.Groups[1].Value;
        //}

        foreach (var error in errorData)
        {
            var errregex = new Regex(error.Regex);
            var errmatch = errregex.Match(wholeLog);
            if (errmatch.Success)
            {
                
                for (var i = 0; i <= 10; i++)
                {
                    error.Solution = error.Solution.Replace("{" + i + "}", errmatch.Groups[i].Value);
                }
                if (log.Errors.All(x => x.ID != error.ID)) log.Errors.Add(error);
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Log Processed...");
        Console.WriteLine("");
        Console.WriteLine($"Current: {log.Current.Count}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Outdated: {log.Outdated.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Broken: {log.Broken.Count}");
        Console.WriteLine($"Library: {log.Library.Count}");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Missing: {log.Missing.Count}");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.WriteLine("");
        Console.ForegroundColor = ConsoleColor.White;
        return log;
    }
}

public class AnalyzedLog
{
    public List<Plugin?> Current { get; set; }
    public List<Plugin?> Outdated { get; set; }
    public List<Plugin?> Broken { get; set; }
    public List<Plugin?> Library { get; set; }
    public List<Plugin?> Missing { get; set; }
    
    public List<Error?> Errors { get; set; }

    public string GTAVersion { get; set; }
    public string RPHVersion { get; set; }
    public string LSPDFRVersion { get; set; }
}