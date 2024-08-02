using System.Text.RegularExpressions;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class Plugins
{
    public static async Task UpdateVersions()
    {
        await UpdateAllVersions();
    }
    
    public static async Task UpdateAllVersions()
    {
        HttpClient webClient = new();
	    var plugins = DbManager.GetPlugins();
        var logMsg = BasicEmbeds.Info($"__Plugin Updates__\r\n*These plugins have updated!*{BasicEmbeds.AddBlanks(45)}\r\n");
        var annMsg = BasicEmbeds.Success($"__Plugin Updates__{BasicEmbeds.AddBlanks(50)}\r\n");
        var upCnt = 0;
        var annCnt = 0;
        
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.Id == 0 || string.IsNullOrEmpty(plugin.Id.ToString()) || plugin.State == State.IGNORE || plugin.State == State.EXTERNAL) continue;
                //Thread.Sleep(3500);

                var onlineVersion = await webClient.GetStringAsync(
                    $"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.Id}&textOnly=1");
                onlineVersion = onlineVersion.Split(" ")[0].Trim();
                onlineVersion = Regex.Replace(onlineVersion, "[^0-9.]", "");

                var onlineVersionSplit = onlineVersion.Split(".");
                if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                if (onlineVersionSplit.Length == 3) onlineVersion += ".0";
                if (plugin.Version == onlineVersion) continue;
                if ( plugin.Announce ) annCnt++;
                upCnt++;

                if ( string.IsNullOrEmpty(plugin.Version) ) plugin.Version = "0";
                if ( string.IsNullOrEmpty(onlineVersion) ) onlineVersion = "0";

                if ( upCnt < 11 )
                {
                    var ea = (!string.IsNullOrEmpty(plugin.EaVersion) && plugin.EaVersion != "0");
                    logMsg.Description +=
                        $"## __[{plugin.Name}]({plugin.Link})__\r\n" +
                        $"> **Previous Version:** `{plugin.Version}`\r\n" +
                        $"> **New Version:** `{onlineVersion}`\r\n" +
                        $"> **Type:** `{plugin.PluginType}` | **State:** `{plugin.State}`\r\n" +
                        $"> **EA Version?:** `{ea}`\r\n";
                }
                
                if ( annCnt < 11 && plugin.Announce )
                {
                    var ea = (!string.IsNullOrEmpty(plugin.EaVersion) && plugin.EaVersion != "0");
                    annMsg.Description +=
                        $"## __[{plugin.Name}]({plugin.Link})__\r\n" +
                        $"> **Previous Version:** `{plugin.Version}`\r\n" +
                        $"> **New Version:** `{onlineVersion}`\r\n" +
                        $"> **Type:** `{plugin.PluginType}` | **State:** `{plugin.State}`\r\n";
                }
                
                Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                plugin.Version = onlineVersion;
                DbManager.EditPlugin(plugin);
            }
            catch (HttpRequestException e)
            {
                await Logging.ErrLog($"{plugin.Name} skipped. Likely hidden on LSPDFR!\r\n\r\n{e}");
            }
            catch (TaskCanceledException e)
            {
                await Logging.ErrLog($"{plugin.Name} skipped. Likely hidden on LSPDFR!\r\n\r\n{e}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logging.ErrLog($"Version Updater Exception:\r\n {e}");
            }
        }

        if ( upCnt == 0 ) return;
        await Logging.SendLog(0, 0, logMsg, false);
        if ( annCnt == 0 ) return;
        var ch = await Program.BotSettings.BotLogs();
        await ch.SendMessageAsync(annMsg);
    }
}