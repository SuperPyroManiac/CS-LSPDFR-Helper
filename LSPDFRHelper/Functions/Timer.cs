using System.Timers;

namespace LSPDFRHelper.Functions;

public static class Timer
{
    public static void Start()
    {
        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(25));
        timer.Elapsed += ShortTimer;
        timer.Start();
        
        var longTimer = new System.Timers.Timer(TimeSpan.FromHours(5));
        longTimer.Elapsed += LongTimer;
        longTimer.Start();
    }

    private static async void ShortTimer(object _, ElapsedEventArgs e)
    {
        await Program.Cache.RemoveExpiredCaches();
        await Verifications.AutoHelper.ValidateOpenCases();
        await Verifications.Plugins.UpdateQuick();
    }
    
    private static async void LongTimer(object _, ElapsedEventArgs e)
    {
        await Verifications.Plugins.UpdateAll();
    }
}
