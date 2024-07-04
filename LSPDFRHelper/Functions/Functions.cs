using DSharpPlus;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions;

public class Functions
{
    private static readonly DiscordClient Client = Program.Client;
    
    public static DiscordGuild GetGuild()
    {
        return Client.Guilds[Program.Settings.ServerId];
    }
    
    public static  async Task<DiscordMember> GetMember(ulong uid)
    {
        return await GetGuild().GetMemberAsync(uid);
    }
    
    public static async Task Blacklist(ulong userId, string reason)
    {
        var user = Program.Cache.GetUser(userId);
        user.Blocked = true;
        DbManager.EditUser(user);
        await Logging.ReportPubLog(BasicEmbeds.Error("__User Blacklisted!__\r\n" + reason));
    }
}