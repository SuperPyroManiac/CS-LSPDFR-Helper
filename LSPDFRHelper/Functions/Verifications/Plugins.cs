using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.Functions.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LSPDFRHelper.Functions.Verifications;

public static class Plugins
{
    private static List<LSPDFRPlugin> _pCache = [];
    
    public static async Task UpdateAll()
    {
	    var plugins = DbManager.GetPlugins();
        var logMsg = BasicEmbeds.Info($"__Plugin Updates__\r\n*These plugins have updated!*{BasicEmbeds.AddBlanks(45)}\r\n");
        var upCnt = 0;
        var skip = 0;
        
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.Id == 0 || string.IsNullOrEmpty(plugin.Id.ToString()) || plugin.State == State.IGNORE || plugin.State == State.EXTERNAL) continue;
                Thread.Sleep(3500);

                var onlineVersion = await new HttpClient().GetStringAsync(
                    $"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.Id}&textOnly=1");
                onlineVersion = onlineVersion.Split(" ")[0].Trim();
                onlineVersion = Regex.Replace(onlineVersion, "[^0-9.]", "");

                var onlineVersionSplit = onlineVersion.Split(".");
                if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                if (onlineVersionSplit.Length == 3) onlineVersion += ".0";
                if (plugin.Version == onlineVersion) continue;
                upCnt++;

                if ( string.IsNullOrEmpty(plugin.Version) ) plugin.Version = "0";
                if ( string.IsNullOrEmpty(onlineVersion) ) onlineVersion = "0";

                if ( upCnt < 11 )
                {
                    var ea = !string.IsNullOrEmpty(plugin.EaVersion) && plugin.EaVersion != "0";
                    logMsg.Description +=
                        $"## __[{plugin.Name}]({plugin.Link})__\r\n" +
                        $"> **Previous Version:** `{plugin.Version}`\r\n" +
                        $"> **New Version:** `{onlineVersion}`\r\n" +
                        $"> **Type:** `{plugin.PluginType}` | **State:** `{plugin.State}`\r\n" +
                        $"> **EA Version?:** `{ea}`\r\n";
                }
                
                Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                plugin.Version = onlineVersion;
                DbManager.EditPlugin(plugin);
            }
            catch ( HttpRequestException )
            {
                if ( skip > 2 )
                {
                    await Logging.ErrLog($"3 updater failures in a row. Likely LSPDFR is down or experiencing issues. Skipping update checks.");
                    return;
                }
                skip++;
                await Logging.ErrLog($"{plugin.Name} skipped. Likely hidden on LSPDFR!");
            }
            catch ( TaskCanceledException )
            {
                if ( skip > 2 )
                {
                    await Logging.ErrLog($"3 updater failures in a row. Likely LSPDFR is down or experiencing issues. Skipping update checks.");
                    return;
                }
                skip++;
                await Logging.ErrLog($"{plugin.Name} skipped. Likely hidden on LSPDFR!");
            }
            catch (Exception e)
            {
                if ( skip > 2 )
                {
                    await Logging.ErrLog($"3 updater failures in a row. Likely LSPDFR is down or experiencing issues. Skipping update checks.");
                    return;
                }
                skip++;
                Console.WriteLine(e);
                await Logging.ErrLog($"Version Updater Exception:\r\n {e}");
            }
        }

        if ( upCnt == 0 ) return;
        await Logging.SendLog(logMsg);
    }
    
    public static async Task UpdateQuick()
    {
        try
        {
            var logMsg = BasicEmbeds.Info($"__Plugin Updates__\r\n*These plugins have updated!*{BasicEmbeds.AddBlanks(45)}\r\n");
            var missingMsg = BasicEmbeds.Info($"__Unknown Plugins__\r\n*These plugins have been found on the site but are not in the DB!*{BasicEmbeds.AddBlanks(45)}\r\n");
            var webPlugs = JsonConvert.DeserializeObject<List<LSPDFRPlugin>>(JObject.Parse(await new HttpClient().GetStringAsync("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=getAllVersions&categoryId=45"))["results"]!.ToString());
            var upCnt = 0;
            
            foreach ( var webPlug in webPlugs )
            {
                foreach ( var plug in Program.Cache.GetPlugins().Where(x => x.Id.ToString() == webPlug.file_id) )
                {
                    if ( plug.Id == 0 || plug.State is State.IGNORE or State.EXTERNAL) continue;
                    
                    webPlug.file_version = webPlug.file_version.Split(" ")[0].Trim();
                    webPlug.file_version = Regex.Replace(webPlug.file_version, "[^0-9.]", "");
                    var onlineVersionSplit = webPlug.file_version.Split(".");
                    if (onlineVersionSplit.Length == 2) webPlug.file_version += ".0.0";
                    if (onlineVersionSplit.Length == 3) webPlug.file_version += ".0";
                    if ( string.IsNullOrEmpty(plug.Version) ) plug.Version = "0";
                    if ( string.IsNullOrEmpty(webPlug.file_version) ) webPlug.file_version = "0";
                    if ( plug.Version.Equals(webPlug.file_version) ) continue;
                    
                    logMsg.Description +=
                        $"## __[{plug.Name}]({plug.Link})__\r\n" +
                        $"> **Previous Version:** `{plug.Version}`\r\n" +
                        $"> **New Version:** `{webPlug.file_version}`\r\n" +
                        $"> **Type:** `{plug.PluginType}` | **State:** `{plug.State}`\r\n" +
                        $"> **EA Version?:** `{!string.IsNullOrEmpty(plug.EaVersion) && plug.EaVersion != "0"}`\r\n";
                    
                    Console.WriteLine($"Updating Plugin {plug.Name} from {plug.Version} to {webPlug.file_version}");
                    plug.Version = webPlug.file_version;
                    DbManager.EditPlugin(plug);
                    upCnt++;
                }
            }
            
            if ( upCnt > 0 ) await Logging.SendLog(logMsg);
            // if ( misgCnt > 0 ) await Logging.SendLog(missingMsg);
        }
        catch ( Exception )
        {
            // ignored
        }
    }
}