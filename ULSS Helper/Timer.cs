using System.Timers;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper;

internal class Timer
{
    internal static void StartTimer()
    {
        var aTimer = new System.Timers.Timer(TimeSpan.FromMinutes(30));
        aTimer.Elapsed += OnLongTimedEvent;
        aTimer.Start();

        var bTimer = new System.Timers.Timer(TimeSpan.FromMinutes(10));
        bTimer.Elapsed += OnShortTimedEvent;
        bTimer.Start();
    }

    private static void OnLongTimedEvent(object source, ElapsedEventArgs e)
    {
        //Update Checker
        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        //Check Cases
        var cases = Database.LoadCases();
        foreach (var ac in cases.Where(x => x.Solved == 0))
        {
            if (ac.Timer > 0) ac.Timer--;
            Database.EditCase(ac);
            Task.Run(CheckCases.Validate);
        }
    }

    private static async void OnShortTimedEvent(object source, ElapsedEventArgs e)
    {
        //Clean & Update Caches
        await Task.Run(() => Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromMinutes(10)));
        await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));
        await Task.Run(() => Program.Cache.UpdateErrors(Database.LoadErrors()));
        await Task.Run(() => Program.Cache.UpdateCases(Database.LoadCases()));
        await Task.Run(() => Program.Cache.UpdateUsers(Database.LoadUsers()));
    }
}