using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CloseCase
{
    internal static async Task Close(AutoCase ac)
    {
        try
        {
            ac.Solved = 1;
            ac.Timer = 0;
            if (ac.TsRequested == 1 && ac.RequestID != null)
            {
                var chTs = await Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId);
                var tmpmsg = await chTs.GetMessageAsync(ulong.Parse(ac.RequestID));
                await chTs.DeleteMessageAsync(tmpmsg);
                ac.RequestID = null;
            }
            Database.EditCase(ac);

            var tmpch = await Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID));
            var ch = (DiscordThreadChannel)tmpch;
            await ch.SendMessageAsync(BasicEmbeds.Warning(
                "__Thread has been archived!__\r\n" +
                "> It is now closed to replies. If you need further help start a new one or ask in the public support channels!", true));
            await ch.ModifyAsync(model => model.Locked = true);
            await ch.ModifyAsync(model => model.IsArchived = true);
        }
        catch (Exception e)
        {
            await Logging.ErrLog($"Ayyoo pyro, the stupid bug happened.\r\nCase: {ac.CaseID}\r\nChanne: <#{ac.ChannelID}>\r\nOwner: <@{ac.OwnerID}>\r\n{e}");
            Console.WriteLine(e);
        }
    }
}