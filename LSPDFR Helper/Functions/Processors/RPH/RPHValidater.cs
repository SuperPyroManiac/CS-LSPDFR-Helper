using System.Text.RegularExpressions;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.CustomTypes.LogTypes;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.Functions.Processors.RPH;

public static class RPHValidater
{
    public static async Task<RPHLog> Run(string attachmentUrl)
    {
        var log = new RPHLog();
        
        var rawLog = await new HttpClient().GetStringAsync(attachmentUrl);

        List<Plugin> unsorted = [];
        log.DownloadLink = attachmentUrl;
        log.LogPath = new Regex(@"Log path: (.+)RagePluginHook\.log").Match(rawLog).Groups[1].Value;
        log.RPHVersion = new Regex(@".+ Version: RAGE Plugin Hook v(\d+\.\d+\.\d+\.\d+) for Grand Theft Auto V").Match(rawLog).Groups[1].Value;
        log.LSPDFRVersion = new Regex(@".+ Running LSPD First Response 0\.4\.9 \((\d+\.\d+\.\d+\.\d+)\)").Match(rawLog).Groups[1].Value;
        log.GTAVersion = new Regex(@".+ Product version: (\d+\.\d+\.\d+\.\d+)").Match(rawLog).Groups[1].Value;
        
        if (!rawLog.Contains("Started new log on") || !rawLog.Contains("Cleaning temp folder"))
        {
            log.LogModified = true;
            log.Errors.Add(new Error
            {
                Id = 666,
                Level = Level.CRITICAL,
                Solution = "**This log has been modified! It is invalid and will not be checked!**"
            });
            return log;
        }
        
        var rphMatch = new Regex(@"Loading plugin .+\wlugins(?:\\|/)(.+).dll.*", RegexOptions.Multiline).Matches(rawLog);
        foreach ( Match match in rphMatch )
        {
            var existingPlug = Program.Cache.GetPlugin(match.Groups[1].ToString());
            if ( existingPlug != null ) if (unsorted.All(x => x.Name != match.Groups[1].ToString())) unsorted.Add(Program.Cache.GetPlugin(match.Groups[1].ToString()));
            if (existingPlug == null && log.Missing.All(x => x.Name != match.Groups[1].ToString()))
                log.Missing.Add(new Plugin {Name = match.Groups[1].ToString()});
        }
        
        var lspdfrMatch = new Regex(@"(?<!CalloutManager\.cs:line 738)\n.+LSPD First Response: (?!無法載入檔案或組件|\[)(.+), Version=(.+), Culture=\w+, PublicKeyToken=\w+", RegexOptions.Multiline).Matches(rawLog);
        foreach ( Match match in lspdfrMatch )
        {
            var existingPlug = Program.Cache.GetPlugin(match.Groups[1].ToString());
            if ( existingPlug != null )
            {
                var newPlug = existingPlug;
                newPlug.Version = match.Groups[2].ToString();
                if (unsorted.All(x => x.Name != newPlug.Name)) unsorted.Add(newPlug);
                continue;
            }
            if (log.Missing.All(x => x.Name != match.Groups[1].ToString()))
                log.Missing.Add(new Plugin {Name = match.Groups[1].ToString()});
        }

        foreach ( var logPlugin in unsorted )
        {
            var plugin = Program.Cache.GetPlugin(logPlugin.Name);
            switch ( logPlugin.State )
            {
                case State.NORMAL:
                case State.EXTERNAL:
                    if ( plugin.EaVersion == logPlugin.EaVersion && log.Current.All(x => x.Name != logPlugin.Name) )
                    {
                        log.Current.Add(logPlugin);
                        break;
                    }
                    
                    var result = CompareVersions(plugin.Version, logPlugin.Version);
                    switch (result)
                    {
                        // plugin version in log is older than version in DB
                        case < 0:
                        {
                            if (log.Outdated.All(x => x.Name != logPlugin.Name)) log.Outdated.Add(logPlugin);
                            break;
                        }
                        // plugin version in log is newer than version in DB and there is no Early Access version
                        case > 0:
                        {
                            logPlugin.EaVersion = plugin.Version;
                            if (log.NewVersion.All(x => x.Name != logPlugin.Name)) log.NewVersion.Add(logPlugin);
                            break;
                        }
                        default:
                        {
                            if (log.Current.All(x => x.Name != logPlugin.Name)) log.Current.Add(logPlugin);
                            break;
                        }
                    }
                    break;
                case State.BROKEN:
                case State.IGNORE:
                    if (log.Current.All(x => x.Name != logPlugin.Name)) log.Current.Add(logPlugin);
                    break;
            }
        }

        foreach (var error in Program.Cache.GetErrors())
        {
            if (error.Id is 1 or 97 or 98 or 99 or 41 or 176) continue;
            if (error.Level is Level.PMSG or Level.PIMG) continue;
            var errMatch = new Regex(error.Pattern).Matches(rawLog);
            foreach (Match match in errMatch)
            {
                var newError = error;
                for (var i = 0; i <= 3; i++)
                    newError.Solution = newError.Solution.Replace("{" + i + "}", match.Groups[i].Value);
                if ( log.Errors.All(x => x.Solution != newError.Solution) ) log.Errors.Add(newError);
            }
        }

        log = RPHSpecialErrors.ProcessSpecialErrors(log, rawLog);
        log.Errors = log.Errors.OrderBy(x => x.Level).ToList();
        
        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).TotalMilliseconds.ToString();
        
        return log;
    }

    private static int CompareVersions(string version1, string version2)
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
}