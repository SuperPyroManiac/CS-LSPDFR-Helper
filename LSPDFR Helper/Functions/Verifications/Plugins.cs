using System.Text.RegularExpressions;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions.Verifications;

public static class Plugins
{
    public static void UpdateVersions()
    {
        var th = new Thread(UpdateThread);
        th.Start();
    }

    private static async void UpdateThread()
    {
                HttpClient webClient = new();
	    var plugins = DbManager.GetPlugins();
        var updatedStr = new List<string>();
        var upBrkStrlist = new List<string>();
        var upEaStrlist = new List<string>();
        var upAnnStrlist = new List<string>();//TODO add announcements!!!!!
        
        foreach (var plugin in plugins)
        {
            try
            {
                if (plugin.Id == 0 || string.IsNullOrEmpty(plugin.Id.ToString()) || plugin.State == State.IGNORE || plugin.State == State.EXTERNAL) continue;
                Thread.Sleep(3500);

                var onlineVersion = await webClient.GetStringAsync(
                    $"https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId={plugin.Id}&textOnly=1");
                onlineVersion = onlineVersion.Split(" ")[0].Trim();
                onlineVersion = Regex.Replace(onlineVersion, "[^0-9.]", "");

                var onlineVersionSplit = onlineVersion.Split(".");
                if (onlineVersionSplit.Length == 2) onlineVersion += ".0.0";
                if (onlineVersionSplit.Length == 3) onlineVersion += ".0";
                if (plugin.Version == onlineVersion) continue;
                
                updatedStr.Add($"{plugin.Name} from `{plugin.Version}` to `{onlineVersion}`");
                if ( plugin.State == State.BROKEN ) upBrkStrlist.Add($"{plugin.Name}");
                if ( !string.IsNullOrEmpty(plugin.EaVersion) && plugin.EaVersion != "0" ) upEaStrlist.Add($"{plugin.Name}");
                if ( plugin.Announce ) upAnnStrlist.Add($"{plugin.Name} from `{plugin.Version}` to `{onlineVersion}`");
                Console.WriteLine($"Updating Plugin {plugin.Name} from {plugin.Version} to {onlineVersion}");
                plugin.Version = onlineVersion;
                DbManager.EditPlugin(plugin);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"{plugin.Name} skipped.\r\n{e}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"{plugin.Name} skipped.\r\n{e}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Logging.ErrLog($"Version Updater Exception:\r\n {e}");
            }
        }
        Program.Cache.UpdatePlugins(DbManager.GetPlugins());

        if ( updatedStr.Count == 0 ) return;
        await Logging.SendLog(0, 0, BasicEmbeds.Info($"__Plugins Have Updated!__\r\n>>> - {string.Join("\r\n- ", updatedStr)}", true), false);
        if (upBrkStrlist.Count > 0) await Logging.SendLog(0, 0, BasicEmbeds.Warning($"__Broken Updates!__\r\n>>> Please review these broken plugins that have updated!\r\n- {string.Join("\r\n- ", upBrkStrlist)}", true), false);
        if (upEaStrlist.Count > 0) await Logging.SendLog(0, 0, BasicEmbeds.Warning($"__Early Access Updates!__\r\n>>> Please review these plugins that contain an Ea version!\r\n- {string.Join("\r\n- ", upEaStrlist)}", true), false);
    }
}