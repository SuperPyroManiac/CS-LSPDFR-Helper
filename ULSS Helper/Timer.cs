using System.Timers;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper;

namespace ULSS_Helper;

internal class Timer
{
    internal static void StartTimer()
    {
        var aTimer = new System.Timers.Timer(TimeSpan.FromHours(1));
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
        
        //Check Cases
        var cases = Database.LoadCases();
        foreach (var ac in cases.Where(x => x.Solved == 0))
        {
            if (ac.Timer > 0) ac.Timer--;
            Database.EditCase(ac);
            if (ac.Timer <= 0) Task.Run(() => CloseCase.Close(ac));
        }
    }

    private static void OnShortTimedEvent(object source, ElapsedEventArgs e)
    {
        //Clean & Update Caches
        Task.Run(() => Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromMinutes(10)));
        Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));
    }
}