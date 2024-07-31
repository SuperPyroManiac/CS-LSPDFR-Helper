using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.Functions.Processors.RPH;

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
        var err1 = Program.Cache.GetError(1).Clone();
        foreach (Match match in new Regex(err1.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (err1.PluginList.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Current.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Outdated.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            err1.PluginList.Add(Program.Cache.GetPlugin(match.Groups[2].Value) ?? new Plugin { Name = match.Groups[2].Value, DName = match.Groups[2].Value});
        }
        if (err1.PluginList.Count > 0)
        {
            var linkedDepend = err1.PluginList.Select(
	            plugin => plugin?.Link != null && plugin.Link.StartsWith("https://")
		            ? $"[{plugin.DName}]({plugin.Link})"
		            : $"[{plugin?.DName}](https://www.google.com/search?q=lspdfr+{plugin!.Name.Replace(" ", "+")})").ToList();
            var linkedDependstring = string.Join("\r\n- ", linkedDepend);
            err1.Solution = $"{err1.Solution}\r\n- {linkedDependstring}";
            if (err1.Solution.Length >= 1023) err1.Solution = "Too many to show!";
            log.Errors.Add(err1);
        }
        
        var libErr = errorData.Find(x => x.Id == 97).Clone();
        foreach (Match match in new Regex(libErr.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (libErr.PluginList.Any(x => x.Name.Equals(match.Groups[1].Value))) continue;
            libErr.PluginList.Add(Program.Cache.GetPlugin(match.Groups[1].Value) ?? new Plugin {Name = match.Groups[1].Value});
        }
        if (libErr.PluginList.Count != 0)
        {
            libErr.Solution = $"{libErr.Solution}\r\n- {string.Join("\r\n- ", libErr.PluginList.Select(x => x.Name).ToList())}";
            if (libErr.Solution.Length >= 1023) libErr.Solution = "Too many to show!";
            log.Errors.Add(libErr);
        }
        
        var scriptErr = errorData.Find(x => x.Id == 98).Clone();
        foreach (Match match in new Regex(scriptErr.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (scriptErr.PluginList.Any(x => x.Name.Equals(match.Groups[1].Value))) continue;
            var errPlug = Program.Cache.GetPlugin(match.Groups[1].Value) ?? new Plugin {Name = match.Groups[1].Value};
            scriptErr.PluginList.Add(errPlug);
        }
        if (scriptErr.PluginList.Count != 0)
        {
            scriptErr.Solution = $"{scriptErr.Solution}\r\n- {string.Join("\r\n- ", scriptErr.PluginList.Select(x => x.Name).ToList())}";
            if (scriptErr.Solution.Length >= 1024) scriptErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(scriptErr);
        }
        
        var plugErr = errorData.Find(x => x.Id == 99).Clone();
        foreach (Match match in new Regex(plugErr.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (plugErr.PluginList.Any(x => x.Name.Equals(match.Groups[1].Value))) continue;
            var errPlug = Program.Cache.GetPlugin(match.Groups[1].Value) ?? new Plugin {Name = match.Groups[1].Value};
            plugErr.PluginList.Add(errPlug);
        }
        if (plugErr.PluginList.Count != 0)
        {
            plugErr.Solution = $"{plugErr.Solution}\r\n- {string.Join("\r\n- ", plugErr.PluginList.Select(x => x.Name).ToList())}";
            if (plugErr.Solution.Length >= 1024) plugErr.Solution = "Too many to show! God damn!";
            log.Errors.Add(plugErr);
        }
        
        var plugOth = errorData.Find(x => x.Id == 41).Clone();
        foreach (Match match in new Regex(plugOth.Pattern, RegexOptions.Multiline).Matches(wholeLog))
        {
            if (plugOth.PluginList.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            var errPlug = Program.Cache.GetPlugin(match.Groups[2].Value) ?? new Plugin {Name = match.Groups[2].Value};
            plugOth.PluginList.Add(errPlug);
        }
        if (plugOth.PluginList.Count != 0)
        {
            plugOth.Solution = $"{plugOth.Solution}\r\n- {string.Join("\r\n- ", plugOth.PluginList.Select(x => x.Name).ToList())}";
            if (plugOth.Solution.Length >= 1024) plugOth.Solution = "Too many to show! God damn!";
            log.Errors.Add(plugOth);
        }
        
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