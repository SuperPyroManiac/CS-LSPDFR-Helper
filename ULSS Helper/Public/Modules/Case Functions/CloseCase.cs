using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.Modules.Case_Functions;

internal class CloseCase
{
    internal static async Task Close(AutoCase ac)
    {
        ac.Solved = 1;
        ac.Timer = 0;
        if (ac.TsRequested == 1 && ac.RequestID != null)
        {
            var chTs = Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId).Result;
            await chTs.DeleteMessageAsync(chTs.GetMessageAsync(ulong.Parse(ac.RequestID)).Result);
            ac.RequestID = null;
        }
        Database.EditCase(ac);
        
        var ch = (DiscordThreadChannel)Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID)).Result;
        await Program.Client.GetChannelAsync(ch.Id).Result.SendMessageAsync(BasicEmbeds.Warning(
            "__Thread has been archived!__\r\n" +
            "> It is now closed to replies. If you need further help start a new one or ask in the public support channels!",
            true));
        await ch.ModifyAsync(model => model.Locked = true);
        await ch.ModifyAsync(model => model.IsArchived = true);
    }
}