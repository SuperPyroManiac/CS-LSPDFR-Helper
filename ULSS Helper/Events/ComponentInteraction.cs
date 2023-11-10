using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ComponentInteraction
{
    public const string RphGetDetailedInfo = "RphGetDetailedInfo";
    public const string RphQuickSendToUser = "RphQuickInfoSendToUser";
    public const string RphDetailedSendToUser = "RphDetailedSendToUser";
    public const string ElsGetDetailedInfo = "ElsGetDetailedInfo";
    public const string ElsQuickSendToUser = "ElsQuickInfoSendToUser";
    public const string ElsDetailedSendToUser = "ElsDetailedSendToUser";

    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        List<string> cacheEventIds = new()
        {
            RphGetDetailedInfo,
            RphQuickSendToUser,
            RphDetailedSendToUser,
            ElsGetDetailedInfo,
            ElsQuickSendToUser,
            ElsDetailedSendToUser,
        };
        try
        {
            if (cacheEventIds.Any(eventId => eventId == e.Id))
            {
                ProcessCache cache = Program.Cache.GetProcessCache(e.Message.Id);

                // RPH log reader buttons
                if (e.Id is RphQuickSendToUser or RphDetailedSendToUser) 
                    await cache.RphProcess.SendMessageToUser(e);
                
                if (e.Id == RphGetDetailedInfo) 
                    await cache.RphProcess.SendDetailedInfoMessage(e);
            
                // ELS log reader buttons
                if (e.Id is ElsQuickSendToUser or ElsDetailedSendToUser)
                    await cache.ElsProcess.SendMessageToUser(e);
                
                if (e.Id == ElsGetDetailedInfo) 
                    await cache.ElsProcess.SendDetailedInfoMessage(e);
            }
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}