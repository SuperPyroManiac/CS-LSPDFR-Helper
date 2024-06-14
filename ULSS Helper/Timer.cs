using System.Timers;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper;

internal class Timer
{
    internal static void StartTimer()
    {
        var aTimer = new System.Timers.Timer(TimeSpan.FromHours(1));
        aTimer.Elapsed += OnLongTimedEvent;
        aTimer.Start();

        var bTimer = new System.Timers.Timer(TimeSpan.FromSeconds(30));
        bTimer.Elapsed += OnShortTimedEvent;
        bTimer.Start();
    }

    private static void OnLongTimedEvent(object source, ElapsedEventArgs e)
    {
        //Update Checker
        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
    }

    private static async void OnShortTimedEvent(object source, ElapsedEventArgs e)
    {
        //Clean & Update Caches
        await Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromMinutes(5));
        await CaseMonitor.UpdateMonitor();
        // Program.Cache.UpdatePlugins(Database.LoadPlugins());
        // Program.Cache.UpdateErrors(Database.LoadErrors());
        // Program.Cache.UpdateCases(Database.LoadCases());
        // Program.Cache.UpdateUsers(Database.LoadUsers());
        
        //Verify Cases & Users
        await Task.Run(CheckUsers.Validate);
        await Task.Run(CheckCases.Validate);
    }
}