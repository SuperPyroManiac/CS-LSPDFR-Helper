using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.Functions.Processors.RPH;

public static class RPHValidater
{
    public static async Task<RPHLog> Run(string attachmentUrl, bool useString = false)
    {
        var log = new RPHLog();
        string rawLog;

        if ( useString ) rawLog = attachmentUrl;
        else rawLog = await new HttpClient().GetStringAsync(attachmentUrl);
        
        List<Plugin> unsorted = [];
        log.DownloadLink = attachmentUrl;
        if ( useString ) log.DownloadLink = "LSPDFR Desktop Helper";
        log.LogPath = new Regex(@"Log path: (.+)RagePluginHook\.log").Match(rawLog).Groups[1].Value;
        log.RPHVersion = new Regex(@".+ Version: RAGE Plugin Hook v(\d+\.\d+\.\d+\.\d+) for Grand Theft Auto V").Match(rawLog).Groups[1].Value;
        log.LSPDFRVersion = new Regex(@".+ Running LSPD First Response 0\.4\.9 \((\d+\.\d+\.\d+\.\d+)\)").Match(rawLog).Groups[1].Value;
        log.GTAVersion = new Regex(@".+ Product version: (\d+\.\d+\.\d+\.\d+)").Match(rawLog).Groups[1].Value;

        if ( !rawLog.Contains("Started new log on") || !rawLog.Contains("Cleaning temp folder") )
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

        var allMatch = new Regex(@"(?:(?<!CalloutManager\.cs:line 738)\n.+LSPD First Response: (?!無法載入檔案或組件|\[|Creating| |Error)\W?(.{1,40}), Version=(.+), Culture=\w{1,10}, PublicKeyToken=\w{1,10})|(?:Loading plugin .+\wlugins(?:\\|/)(.+).dll.*)", RegexOptions.Multiline).Matches(rawLog);
        foreach ( Match match in allMatch )
        {
            if ( match.Groups[1].Value.Length > 0 )
            {
                var plug = Program.Cache.GetPlugin(match.Groups[1].Value);
                if ( plug != null )
                {
                    var newPlug = plug.Clone();
                    newPlug.Version = match.Groups[2].Value;
                    if ( unsorted.All(x => x.Name != newPlug.Name) ) unsorted.Add(newPlug);
                    continue;
                }
                if ( log.Missing.All(x => x.Name != match.Groups[1].Value) )
                    log.Missing.Add(new Plugin { Name = match.Groups[1].Value, Version = match.Groups[2].Value});
                continue;
            }
            //RPH Plugins
            var existingRphPlug = Program.Cache.GetPlugin(match.Groups[3].Value);
            if ( existingRphPlug != null ) if ( unsorted.All(x => x.Name != match.Groups[3].Value) ) unsorted.Add(Program.Cache.GetPlugin(match.Groups[3].Value));
            if ( existingRphPlug == null && log.Missing.All(x => x.Name != match.Groups[3].Value) ) log.Missing.Add(new Plugin { Name = match.Groups[3].Value, PluginType = PluginType.RPH, Version = "RPH"});
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
                    switch ( result )
                    {
                        // plugin version in log is older than version in DB
                        case < 0:
                        {
                            if ( log.Outdated.All(x => x.Name != logPlugin.Name) ) log.Outdated.Add(logPlugin);
                            break;
                        }
                        // plugin version in log is newer than version in DB and there is no Early Access version
                        case > 0:
                        {
                            logPlugin.EaVersion = plugin.Version;
                            if ( log.NewVersion.All(x => x.Name != logPlugin.Name) ) log.NewVersion.Add(logPlugin);
                            break;
                        }
                        default:
                        {
                            if ( log.Current.All(x => x.Name != logPlugin.Name) ) log.Current.Add(logPlugin);
                            break;
                        }
                    }
                    break;
                case State.BROKEN:
                case State.IGNORE:
                    if ( log.Current.All(x => x.Name != logPlugin.Name) ) log.Current.Add(logPlugin);
                    break;
            }
        }

        foreach ( var error in Program.Cache.GetErrors() )
        {
            if ( error.Id is 1 or 97 or 98 or 99 or 41 or 176 ) continue;
            if ( error.Level is Level.PMSG or Level.PIMG ) continue;
            if ( error.StringMatch )
            {
                if (rawLog.Contains(error.Pattern)) log.Errors.Add(error);
                continue;
            }
            
            var errMatch = new Regex(error.Pattern).Matches(rawLog);
            foreach ( Match match in errMatch )
            {
                var newError = error.Clone();
                for ( var i = 0; i <= 3; i++ )
                    newError.Solution = newError.Solution.Replace("{" + i + "}", match.Groups[i].Value);
                if ( log.Errors.All(x => x.Solution != newError.Solution) ) log.Errors.Add(newError);
            }
        }

        log = RPHSpecialErrors.ProcessSpecialErrors(log, rawLog);
        log.Errors = log.Errors.OrderByDescending(x => x.Level).ToList();

        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).Milliseconds.ToString();

        return log;
    }

    private static int CompareVersions(string version1, string version2)
    {
        var parts1 = version1.Split('.');
        var parts2 = version2.Split('.');
        var minLength = Math.Min(parts1.Length, parts2.Length);

        for ( var i = 0; i < minLength; i++ )
        {
            var part1 = int.Parse(parts1[i]);
            var part2 = int.Parse(parts2[i]);
            if ( part1 < part2 ) return -1; // version1 is smaller
            if ( part1 > part2 ) return 1; // version1 is larger
        }
        // If all common parts are equal, check the remaining parts
        if ( parts1.Length < parts2.Length ) return -1; // version1 is smaller
        if ( parts1.Length > parts2.Length ) return 1; // version1 is larger
        return 0; // versions are equal
    }
}