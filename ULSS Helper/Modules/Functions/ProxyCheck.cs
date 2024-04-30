using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Modules.Functions;

public class ProxyCheck
{
    internal static bool Run(RPHLog log, DiscordUser user, DiscordMessage msg)
    {
        if (user.BotEditor == 1 || user.BotAdmin == 1) return true;
        if (string.IsNullOrEmpty(log.LogPath)) return true;
        if (user.LogPath == null)
        {
            user.LogPath = log.LogPath;
            Database.EditUser(user);
            return true;
        }
        if (user.LogPath == log.LogPath) return true;

        var e = new DiscordEmbedBuilder();
        e.Color = DiscordColor.Red;
        e.Description = 
            "### :no_entry: __Possible Proxy Support!__\r\n" +
            ">>> **User uploaded a log with a different path than usual.**\r\n" +
            $"User: <@{user.UID}> ({user.Username})\r\n" +
            $"Log: {msg.JumpLink}";
        e.AddField("Original Path:", user.LogPath);
        e.AddField("New Path:", log.LogPath);

        user.LogPath = log.LogPath;
        Database.EditUser(user);

        Logging.ReportPubLog(e).GetAwaiter();
        return false;
    }
}