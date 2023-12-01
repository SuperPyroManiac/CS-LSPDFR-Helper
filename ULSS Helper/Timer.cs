using System.Timers;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Timer
{
    public const int HourInterval = 3;
    internal static void StartTimer()
    {
        var timer = new System.Timers.Timer(TimeSpan.FromHours(HourInterval).TotalMilliseconds);
        timer.Elapsed += OnTimedEvent;
        timer.Start();
    }
    private static void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        DiscordEmbedBuilder embed = BasicEmbeds.Info(
            msg: $"Running scheduled tasks now\n"
                + $"Current schedule: every {HourInterval} hours\n"
                + $"Tasks:\n"
                + $"- update LSPDFR plugin versions in DB using the lcpdfr.com API\n"
                + $"- create a DB backup\n"
                + $"- clear expired entries in the bot's cache\n",
            bold: true
        );
        new DiscordMessageBuilder()
	        .WithEmbed(embed)
	        .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId).Result);

        // Update LSPDFR plugin versions in DB using the lcpdfr.com API
        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        // Backup DB
        File.Copy(Program.Settings.DbPath, Settings.GenerateNewFilePath(FileType.DB_BACKUP));

        // Clean Cache
        Task.Run(() => Program.Cache.RemoveExpiredCacheEntries(TimeSpan.FromHours(HourInterval)));
    }
}