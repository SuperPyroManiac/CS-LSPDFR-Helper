using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.AutoHelper;

public static class JoinCase
{
    public static async Task<bool> Join(AutoCase ac, ulong user, ulong guild)
    {
        var tmpCh = await Program.Client.GetChannelAsync(ac.ChannelId);
        var ch = (DiscordThreadChannel)tmpCh;
        var users = await ch.ListJoinedMembersAsync();
        foreach ( var usr in users ) if ( usr.Id == user ) return false;
        
        var tsCh = await Program.Client.GetChannelAsync(Program.Cache.GetServer(guild).MonitorChId);
        var isManager = await Program.Cache.GetUser(user).IsManager(guild);
        if (ac.TsRequested && ac.RequestId != 0 && isManager)
        {
            var helpMsg = await tsCh.GetMessageAsync(ac.RequestId);
            await tsCh.DeleteMessageAsync(helpMsg);
            ac.RequestId = 0;
            DbManager.EditCase(ac);
        }

        var mem = await Program.Client.Guilds[guild].GetMemberAsync(user);
        await ch.AddThreadMemberAsync(mem);
        if (isManager) await ch.SendMessageAsync(BasicEmbeds.Success($"__Staff has joined!__\r\n> <@{user}> is here to help!"));
        else await ch.SendMessageAsync(BasicEmbeds.Success($"__User has joined!__\r\n> <@{user}> has joined this case!"));
        return true;
    }
}