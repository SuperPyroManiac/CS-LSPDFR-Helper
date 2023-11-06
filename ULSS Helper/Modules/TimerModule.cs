using System.Timers;
using Timer = System.Timers.Timer;

namespace ULSS_Helper.Modules;

internal class TimerModule
{
    internal static void StartTimer()
    {
        var aTimer = new System.Timers.Timer(60 * 60 * 3000); //3 hours
        aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
        aTimer.Start();
    }
    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        //Update Checker
        var th = new Thread(DatabaseManager.UpdatePluginVersions);
        th.Start();
        
        //Backup DB
        File.Copy(Settings.dbpath, Settings.DbBackupNamer());
    }
}