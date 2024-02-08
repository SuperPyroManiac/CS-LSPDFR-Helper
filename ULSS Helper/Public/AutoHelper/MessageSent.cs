using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        try
        {
            if (ctx.Channel.IsPrivate) return;
            if (Database.LoadCases().Any(x => x.ChannelID == ctx.Channel.Id.ToString()) && !ctx.Author.IsBot)
                await ExtraUploads.CheckLog(ctx);
        }
        catch (Exception e)
        {
            Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}