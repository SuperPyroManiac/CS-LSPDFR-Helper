using System.Timers;

namespace LSPDFRHelper.Functions;

public static class Timer
{
    public static void Start()
    {
        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(15));
        timer.Elapsed += ShortTimer;
        timer.Start();
        
        var longTimer = new System.Timers.Timer(TimeSpan.FromHours(2));
        longTimer.Elapsed += LongTimer;
        longTimer.Start();
    }

    private static async void ShortTimer(object _, ElapsedEventArgs e)
    {
        Task.WaitAll(Program.Cache.RemoveExpiredCaches(), Verifications.AutoHelper.ValidateOpenCases());
        //TODO: PLUGIN VERSION CHECKER
        await Verifications.AutoHelper.UpdateAhMonitor();
    }
    
    private static async void LongTimer(object _, ElapsedEventArgs e)
    {
        await Verifications.Plugins.UpdateAllVersions();
    }
}