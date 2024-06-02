using DSharpPlus;
using DSharpPlus.Entities;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CloseCase
{
    internal static async Task Close(AutoCase ac, bool validate = true)
    {
        try
        {
            ac.Solved = 1;
            ac.Timer = 0;
            if (ac.TsRequested == 1 && ac.RequestID != null)
            {
                var chTs = await Program.Client.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId);
                var msg = await chTs.GetMessageAsync(ulong.Parse(ac.RequestID));
                await chTs.DeleteMessageAsync(msg);
                ac.RequestID = null;
            }

            var tmp = await Program.Client.GetChannelAsync(ulong.Parse(ac.ChannelID));
            var ch = (DiscordThreadChannel)tmp;
            var send = true;

            await foreach (var msg in tmp.GetMessagesAsync(5))
            {
                if (msg.Embeds.Count <= 0) continue;
                if (msg.Embeds[0].Description.Contains("If you need further help start a new one or ask"))
                {
                    send = false;
                    break;
                }
            }
            
            if (send)
                await ch.SendMessageAsync(BasicEmbeds.Warning(
                "__Thread has been archived!__\r\n" +
                "> It is now closed to replies. If you need further help start a new one or ask in the public support channels!", true));
            
            await ch.ModifyAsync(model => model.Locked = true);
            await ch.ModifyAsync(model => model.IsArchived = true);

            await Database.EditCase(ac);
            if (validate) await CheckCases.Validate();
        }
        catch (Exception e)
        {
            await Logging.ErrLog($"Pyro, the stupid bug happened.\r\nCase: {ac.CaseID}\r\nChannel: <#{ac.ChannelID}>\r\nOwner: <@{ac.OwnerID}>\r\n{e}");
            //TODO: Change this message.
            Console.WriteLine(e);
        }
    }
}