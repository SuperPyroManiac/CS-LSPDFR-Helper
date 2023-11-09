using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        try
        {
            ProcessCache cache = Program.Cache.GetProcessCache(e.Message.Id);
            //RPH log reader buttons
            if (e.Id is "send" or "send2") await cache.RphProcess.SendMessageToUser(e);
            if (e.Id == "info") await cache.RphProcess.SendDetailedInfoMessage(e);
        
            //ELS log reader buttons
            if (e.Id is "sendElsToUser" or "sendElsDetailsToUser") await cache.ElsProcess.SendMessageToUser(e);
            if (e.Id == "elsDetails") await cache.ElsProcess.SendDetailedInfoMessage(e);
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}