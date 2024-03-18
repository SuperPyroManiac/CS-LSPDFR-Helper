using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class JoinCase
{
    internal static async Task Join(AutoCase ac, string tsID)
    {
        if (ac.TsRequested == 1 && ac.RequestID != null)
        {
            var chTs = await Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId);
            var tmpmsg = await chTs.GetMessageAsync(ulong.Parse(ac.RequestID));
            await chTs.DeleteMessageAsync(tmpmsg);
            ac.RequestID = null;
            Database.EditCase(ac);
        }

        var tmpch = await Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID));
        var ch = (DiscordThreadChannel)tmpch;
        await ch.SendMessageAsync(BasicEmbeds.Success(
            $"__TS has joined!__\r\n" +
            $"> <@{tsID}> is here to help!", true));
        var tmpusr = await Program.Client.Guilds[Program.Settings.Env.ServerId].GetMemberAsync(ulong.Parse(tsID));
        await ch.AddThreadMemberAsync(tmpusr);
    }
}