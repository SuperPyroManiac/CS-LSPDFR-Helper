using ULSS_Helper.Messages;

namespace ULSS_Helper.Modules.Functions;

public class AutoBlacklist
{
    public static void Add(string userId, string reason)
    {
        var user = Program.Cache.GetUser(userId);
        user.Blocked = 1;
        Database.EditUser(user);
        
        var e = BasicEmbeds.Error("__User AutoBlacklisted!__\r\n", true);
        e.Description = e.Description + reason;
        
        Logging.ReportPubLog(e).GetAwaiter();
    }
}