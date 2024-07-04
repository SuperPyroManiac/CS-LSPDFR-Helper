using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.Functions.Processors.ASI;

public class ASIValidater
{
    public static async Task<ASILog> Run(string attachmentUrl)
    {
        var log = new ASILog();
        log.DownloadLink = attachmentUrl;
        var wholeLog = await new HttpClient().GetStringAsync(attachmentUrl);
        var unknownPlugins = new List<Plugin>();

        foreach ( Match match in new Regex(@"^\s+.(.+.asi). failed to load.*", RegexOptions.Multiline).Matches(wholeLog) )
        {
            var plug = Program.Cache.GetPlugin(match.Groups[1].Value);
            if ( plug == null )
            {
                plug = new Plugin { Name = match.Groups[1].Value, Version = "ASI", PluginType = PluginType.ASI };
                log.Missing.Add(plug);
            }
            if (plug.State == State.BROKEN) log.BrokenAsiFiles.Add(plug.Name);
            log.FailedAsiFiles.Add(plug);
        }
        
        foreach ( Match match in new Regex(@"^\s+.(.+.asi). (?!failed to load).*", RegexOptions.Multiline).Matches(wholeLog) )
        {
            var plug = Program.Cache.GetPlugin(match.Groups[1].Value);
            if ( plug == null )
            {
                plug = new Plugin { Name = match.Groups[1].Value, Version = "ASI", PluginType = PluginType.ASI };
                log.Missing.Add(plug);
            }
            if (plug.State == State.BROKEN) log.BrokenAsiFiles.Add(plug.Name);
            log.LoadedAsiFiles.Add(plug);
        }
        
        log.ValidaterCompletedAt = DateTime.Now;
        log.ElapsedTime = DateTime.Now.Subtract(log.ValidaterStartedAt).Milliseconds.ToString();

        return log;
    }
}