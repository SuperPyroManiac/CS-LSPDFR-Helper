using System.Timers;
using ULSS_Helper.Events;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Timer
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
        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        //Backup DB
        File.Copy(Program.Settings.DbPath, Settings.GenerateNewFilePath(FileType.DB_BACKUP));
    }
}