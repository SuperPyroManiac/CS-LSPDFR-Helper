using System.Text.RegularExpressions;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.CustomTypes.LogTypes;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.Functions.Processors.RPH;

public static class RPHSpecialErrors
{
    public static RPHLog ProcessSpecialErrors(RPHLog log, string wholeLog)
    {
        var errorData = Program.Cache.GetErrors();
        var pluginData = Program.Cache.GetPlugins();
        
        //===//===//===////===//===//===////===//Multi Session Detection//===////===//===//===////===//===//===//
        if (new Regex("Started loading LSPDFR").Matches(wholeLog).Count > 1)
        {
            log.Errors.Add(new Error
            {
                Id = 15,
                Solution = "**Multiple Sessions**\r\nYour log contains multiple sessions. This means you reloaded LSPDFR without restarting the game. The log reader may provide incorrect results!",
                Level = Level.WARN
            });
        }
        
        //===//===//===////===//===//===////===//Combined Error Lists//===////===//===//===////===//===//===//
        var err1 = Program.Cache.GetError(1);
        foreach (Match match in new Regex(@err1.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (err1.PluginList.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Current.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Outdated.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            var errPlug = Program.Cache.GetPlugin(match.Groups[2].Value);
            if ( errPlug == null ) errPlug = new Plugin() { Name = match.Groups[2].Value };
            err1.PluginList.Add(errPlug);
        }
        if (err1.PluginList.Count > 0)
        {
            var linkedDepend = err1.PluginList.Select(
	            plugin => plugin?.Link != null && plugin.Link.StartsWith("https://")
		            ? $"[{plugin.DName}]({plugin.Link})"
		            : $"[{plugin?.DName}](https://www.google.com/search?q=lspdfr+{plugin.Name.Replace(" ", "+")})").ToList();
            var linkedDependstring = string.Join("\r\n- ", linkedDepend);
            var dependErr = err1;
            dependErr.Solution = $"{err1.Solution}\r\n- {linkedDependstring}";
            if (dependErr.Solution.Length >= 1023) dependErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(dependErr);
        }
        
        // var libErr = errorData.Find(x => x.ID == "97");
        // var libssmatch = new Regex(@libErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
        // foreach (Match match in libssmatch)
        // {
        //     if (log.IncorrectLibs.Any(x => x.Equals(match.Groups[1].Value))) continue;
        //     log.IncorrectLibs.Add(match.Groups[1].Value);
        // }
        // if (log.IncorrectLibs.Count != 0)
        // {
        //     libErr.Solution = $"{libErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectLibs)}";
        //     if (libErr.Solution.Length >= 1024) libErr.Solution = "Too many to show! God damn!";
        //     log.Errors.Add(libErr);
        // }
        //
        // var scriptErr = errorData.Find(x => x.ID == "98");
        // var scriptsmatch = new Regex(@scriptErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
        // foreach (Match match in scriptsmatch)
        // {
        //     if (log.IncorrectScripts.Any(x => x.Equals(match.Groups[1].Value))) continue;
        //     log.IncorrectScripts.Add(match.Groups[1].Value);
        // }
        // if (log.IncorrectScripts.Count != 0)
        // {
        //     scriptErr.Solution = $"{scriptErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectScripts)}";
        //     if (scriptErr.Solution.Length >= 1024) scriptErr.Solution = "Too many to show! God damn!";
        //     log.Errors.Add(scriptErr);
        // }
        //
        // var plugErr = errorData.Find(x => x.ID == "99");
        // var plugssmatch = new Regex(@plugErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
        // foreach (Match match in plugssmatch)
        // {
        //     if (log.IncorrectPlugins.Any(x => x.Equals(match.Groups[1].Value))) continue;
        //     log.IncorrectPlugins.Add(match.Groups[1].Value);
        // }
        // if (log.IncorrectPlugins.Count != 0)
        // {
        //     plugErr.Solution = $"{plugErr.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectPlugins)}";
        //     if (plugErr.Solution.Length >= 1024) plugErr.Solution = "Too many to show! God damn!";
        //     log.Errors.Add(plugErr);
        // }
        //
        // var plugOth = errorData.Find(x => x.ID == "41");
        // var othsmatch = new Regex(@plugOth.Regex, RegexOptions.Multiline).Matches(wholeLog);
        // foreach (Match match in othsmatch)
        // {
        //     if (log.IncorrectOther.Any(x => x.Equals(match.Groups[2].Value))) continue;
        //     log.IncorrectOther.Add(match.Groups[2].Value);
        // }
        // if (log.IncorrectOther.Count != 0)
        // {
        //     plugOth.Solution = $"{plugOth.Solution}\r\n- {string.Join("\r\n- ", log.IncorrectOther)}";
        //     if (plugOth.Solution.Length >= 1024) plugOth.Solution = "Too many to show! God damn!";
        //     log.Errors.Add(plugOth);
        // }
        //
        // //===//===//===////===//===//===////===//Outdated RPH Plugins to Outdated list//===////===//===//===////===//===//===//
        // var rph1Match = Regex.Match(wholeLog, @"DamageTrackingFramework: \[VERSION OUTDATED\]");
        // if (rph1Match.Success && log.Outdated.All(x => x.Name != "DamageTrackingFramework")) 
        //     log.Outdated.Add(
        //         new Plugin
        //         {
        //             Name = "DamageTrackingFramework", 
        //             DName = "DamageTrackingFramework", 
        //             Link = "https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/"
        //         });
        //
        // //===//===//===////===//===//===////===//Exception Detection//===////===//===//===////===//===//===//
        // var crashMatch = Regex.Matches(wholeLog, @"Stack trace:.*\n(?:.+at (\w+)\..+\n)+");
        // var causedCrash = new List<Plugin>();
        // var causedCrashName = new List<string>();
        // foreach (Match match in crashMatch)
        // {
        //     for (var i = match.Groups.Count; i > 0; i--)
        //     {
        //         foreach (Capture capture in match.Groups[i].Captures)
        //         {
        //             if (causedCrash.Any(x => x.Name.Equals(capture.Value))) continue;
        //             foreach (var plugin in pluginData.Where(plugin => plugin.Name.Equals(capture.Value) && plugin.State is "LSPDFR" or "EXTERNAL" or "BROKEN"))
        //             {
        //                 if (!causedCrash.Contains(plugin) || !log.IncorrectLibs.Contains(plugin.Name) || 
        //                     !log.IncorrectOther.Contains(plugin.Name) || !log.IncorrectPlugins.Contains(plugin.Name) || 
        //                     !log.IncorrectScripts.Contains(plugin.Name))
        //                 {
        //                     causedCrash.Add(plugin);
        //                     if (plugin.State is "LSPDFR" or "EXTERNAL" && !string.IsNullOrEmpty(plugin.Link))
        //                         causedCrashName.Add($"[{plugin.DName}]({plugin.Link})");
        //                     else causedCrashName.Add(plugin.DName);
        //                 }
        //             }
        //         }
        //     }
        // }
        // if (causedCrashName.Count > 0)
        //     log.Errors.Add(new Error
        //     {
        //     ID = "20",
        //     Solution = "**These plugins threw an error:**" +
        //                $"\r\n- {string.Join("\r\n- ", causedCrashName)}" +
        //                "\r\nEnsure you follow all other steps listed to fix these!" +
        //                "\r\n*If the issue persists, you may want to report it to the author.*",
        //     Level = "SEVERE"
        //     });
        
        return log;
    }
}