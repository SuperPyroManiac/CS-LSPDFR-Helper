using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions.AutoHelper;

public static class JoinCase
{
    public static async Task<bool> Join(AutoCase ac, DiscordMember user)
    {
        var tmpCh = await Program.Client.GetChannelAsync(ac.ChannelId);
        var ch = (DiscordThreadChannel)tmpCh;
        var users = await ch.ListJoinedMembersAsync();
        if (users.FirstOrDefault(usr => usr.Id == user.Id) != null) return false;
        
        var tsCh = await Program.Client.GetChannelAsync(Program.Settings.MonitorChId);
        var isTs = await Program.Cache.GetUser(user.Id).IsTs();
        if (ac.TsRequested && ac.RequestId != 0 && isTs)
        {
            var helpMsg = await tsCh.GetMessageAsync(ac.RequestId);
            await tsCh.DeleteMessageAsync(helpMsg);
            ac.RequestId = 0;
            DbManager.EditCase(ac);
        }
        
        await ch.AddThreadMemberAsync(user);
        if (isTs) await ch.SendMessageAsync(BasicEmbeds.Success($"__TS has joined!__\r\n> <@{user.Id}> is here to help!"));
        else await ch.SendMessageAsync(BasicEmbeds.Success($"__User has joined!__\r\n> <@{user.Id}> has joined this case!"));
        return true;
    }
}