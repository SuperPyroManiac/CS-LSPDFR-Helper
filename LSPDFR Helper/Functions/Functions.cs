using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace LSPDFR_Helper.Functions;

internal class Functions
{
    private static readonly DiscordClient Client = Program.Client;
    
    internal static DiscordGuild GetGuild()
    {
        return Client.Guilds[Program.Settings.ServerId];
    }
    
    internal static  async Task<DiscordMember> GetMember(string uid)
    {
        return await GetGuild().GetMemberAsync(ulong.Parse(uid));
    }
}