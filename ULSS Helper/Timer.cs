using System.Timers;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Timer
{
    internal static void StartTimer()
    {
        var aTimer = new System.Timers.Timer(TimeSpan.FromHours(2));
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
        
        //Backup DB
        File.Copy(Program.Settings.DbPath, Settings.GenerateNewFilePath(FileType.DB_BACKUP));
    }

    private static void OnShortTimedEvent(object source, ElapsedEventArgs e)
    {
        //Clean Cache
        Task.Run(() => Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromMinutes(10)));
    }


}