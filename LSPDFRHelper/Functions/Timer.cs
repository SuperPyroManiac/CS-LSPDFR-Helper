using System.Timers;

namespace LSPDFRHelper.Functions;

public static class Timer
{
    public static void Start()
    {
        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(5));
        timer.Elapsed += ShortTimer;
        timer.Start();
    }

    private static async void ShortTimer(object _, ElapsedEventArgs e)
    {
        Task.WaitAll(Program.Cache.RemoveExpiredCaches(), Verifications.AutoHelper.ValidateOpenCases());
        //TODO: PLUGIN VERSION CHECKER
        await Verifications.AutoHelper.UpdateAhMonitor();
        
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