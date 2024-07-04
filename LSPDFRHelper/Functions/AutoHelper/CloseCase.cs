using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.AutoHelper;

public static class CloseCase
{
    public static async Task Close(AutoCase ac)
    {
        try
        {
            ac.Solved = true;
            ac.ExpireDate = DateTime.Now.ToUniversalTime();
            if (ac.TsRequested && ac.RequestId != 0)
            {
                var chTs = await Program.Client.GetChannelAsync(Program.Settings.MonitorChId);
                var msg = await chTs.GetMessageAsync(ac.RequestId);
                await chTs.DeleteMessageAsync(msg);
                ac.RequestId = 0;
            }

            var tmp = await Program.Client.GetChannelAsync(ac.ChannelId);
            var ch = (DiscordThreadChannel)tmp;
            var send = true;

            await foreach (var msg in tmp.GetMessagesAsync(5))
            {
                if (msg.Embeds.Count <= 0) continue;
                if (msg.Embeds[0].Description != null && msg.Embeds[0].Description.Contains("If you need further help start a new one or ask"))
                {
                    send = false;
                    break;
                }
            }
            
            if (send)
                await ch.SendMessageAsync(BasicEmbeds.Warning(
                    "__Thread has been archived!__\r\n" +
                    "> It is now closed to replies. If you need further help start a new one or ask in the public support channels!"));
            
            await ch.ModifyAsync(model => model.Locked = true);
            await ch.ModifyAsync(model => model.IsArchived = true);

            DbManager.EditCase(ac);
        }
        catch (Exception e)
        {
            await Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
        }
    }
}