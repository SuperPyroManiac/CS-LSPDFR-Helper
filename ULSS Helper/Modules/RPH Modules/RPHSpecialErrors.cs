using System.Text.RegularExpressions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.RPH_Modules;

public class RPHSpecialErrors
{
    internal static RPHLog ProcessSpecialErrors(RPHLog log, string wholeLog)
    {
        var errorData = Database.LoadErrors();
        var pluginData = Program.Cache.GetPlugins();
        
        //===//===//===////===//===//===////===//Multi Session Detection//===////===//===//===////===//===//===//
        var seshmatch = new Regex("LSPD First Response: Creating plugin").Matches(wholeLog);
        if (seshmatch.Count > 1)
        {
            log.Errors.Add(new Error
            {
                ID = "15",
                Solution = "**Multiple Sessions**" +
                           "\r\nYour log contains multiple sessions. This means you reloaded LSPDFR without restarting the game. " +
                           "This is not an issue, but the log reader cannot provide correct info." +
                           "\r\nPlugin info is based off the first game session." +
                           "\r\nError info can only be provided from a single session.",
                Level = "CRITICAL"
            });
        }
        
        //===//===//===////===//===//===////===//Combined Error Lists//===////===//===//===////===//===//===//
        var dependmatch = new Regex(@errorData[0].Regex, RegexOptions.Multiline).Matches(wholeLog);
        foreach (Match match in dependmatch)
        {
            if (log.MissingDepend.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Current.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Broken.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;
            if (log.Outdated.Any(x => x.Name.Equals(match.Groups[2].Value))) continue;//TODO fuck me
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
        var libssmatch = new Regex(@libErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
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
        var scriptsmatch = new Regex(@scriptErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
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
        var plugssmatch = new Regex(@plugErr.Regex, RegexOptions.Multiline).Matches(wholeLog);
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
        var othsmatch = new Regex(@plugOth.Regex, RegexOptions.Multiline).Matches(wholeLog);
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
        
        //===//===//===////===//===//===////===//Exception Detection//===////===//===//===////===//===//===//
        // var plugExc = errorData.Find(x => x.ID == "176");
        // var excsmatch = new Regex(plugExc.Regex, RegexOptions.Multiline).Matches(wholeLog);
        // foreach (Match match in excsmatch)
        // {
        //     var excErr = plugExc;
        //     for (var i = 0; i <= 10; i++)
        //     {
        //         excErr.Solution = excErr.Solution.Replace("{" + i + "}", match.Groups[i].Value);
        //     }
        //     log.Errors.Add(excErr);
        // }

        //===//===//===////===//===//===////===//RNUI Dupes//===////===//===//===////===//===//===//
        var rmvdupe = new List<Error>();
        if (log.Errors.Any(x => x.ID == "113"))
            foreach (var error in log.Errors)
            {
                if (error.ID is "120" or "131")
                    rmvdupe.Add(error);
            }
        foreach (var dupe in rmvdupe) 
            log.Errors.Remove(dupe);
        
        return log;
    }
}