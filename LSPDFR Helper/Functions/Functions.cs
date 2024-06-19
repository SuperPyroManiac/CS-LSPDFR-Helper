using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace LSPDFR_Helper.Functions;

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
}