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
            var chTs = Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId).Result;
            await chTs.DeleteMessageAsync(chTs.GetMessageAsync(ulong.Parse(ac.RequestID)).Result);
            ac.RequestID = null;
            Database.EditCase(ac);
        }
        var ch = (DiscordThreadChannel)Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID)).Result;
        await Program.Client.GetChannelAsync(ch.Id).Result.SendMessageAsync(BasicEmbeds.Success(
            $"__TS has joined!__\r\n" +
            $"> <@{tsID}> is here to help!", true));
        await ch.AddThreadMemberAsync(Program.Client.Guilds[Program.Settings.Env.ServerId].GetMemberAsync(ulong.Parse(tsID)).Result);
    }
}