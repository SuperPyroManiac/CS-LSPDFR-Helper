using DSharpPlus;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions;

public class Functions
{
    private static readonly DiscordClient Client = Program.Client;
    
    public static DiscordGuild GetGuild(ulong id)
    {
        return Client.Guilds[id];
    }
    
    public static  async Task<DiscordMember> GetMember(ulong guildid, ulong memberid)
    {
        return await GetGuild(guildid).GetMemberAsync(memberid);
    }
    
    public static async Task Blacklist(ulong userId, string reason)
    {
        var user = Program.Cache.GetUser(userId);
        user.Blocked = true;
        DbManager.EditUser(user);
        await Logging.ReportPubLog(BasicEmbeds.Error("__User Blacklisted!__\r\n" + reason));
    }
}