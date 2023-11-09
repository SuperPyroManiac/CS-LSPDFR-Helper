using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Events;

public class ButtonPress
{
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        try
        {
            //RPH log reader buttons
            if (e.Id is "send" or "send2") await RPHProcess.SendMessageToUser(e);
            if (e.Id == "info") await RPHProcess.SendDetailedInfoMessage(e);
        
            //ELS log reader buttons
            if (e.Id is "sendElsToUser" or "sendElsDetailsToUser") await ELSProcess.SendMessageToUser(e);
            if (e.Id == "elsDetails") await ELSProcess.SendDetailedInfoMessage(e);
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}