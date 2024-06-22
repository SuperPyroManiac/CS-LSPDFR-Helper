using System.Timers;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Functions;

public static class Timer
{
    public static void Start()
    {
        var aTimer = new System.Timers.Timer(TimeSpan.FromHours(1));
        aTimer.Elapsed += HourlyTimer;
        aTimer.Start();

        var bTimer = new System.Timers.Timer(TimeSpan.FromSeconds(30));
        bTimer.Elapsed += ShortTimer;
        bTimer.Start();
    }

    private static void HourlyTimer(object _, ElapsedEventArgs e)
    {
        //Update Checker
        var th = new Thread(Plugins.UpdateVersions);
        th.Start();
    }

    private static void ShortTimer(object _, ElapsedEventArgs e)
    {
        //Clean & Update Caches
        //await Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromMinutes(5));
        //await CaseMonitor.UpdateMonitor();
        // Program.Cache.UpdatePlugins(Database.LoadPlugins());
        // Program.Cache.UpdateErrors(Database.LoadErrors());
        // Program.Cache.UpdateCases(Database.LoadCases());
        // Program.Cache.UpdateUsers(Database.LoadUsers());
        
        //Verify Cases & Users
        //await Task.Run(CheckUsers.Validate);
        //await Task.Run(CheckCases.Validate);
    }
}