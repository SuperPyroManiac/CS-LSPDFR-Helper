using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Modules.LogAnalyzer;
using ULSS_Helper.Modules.Messages;

namespace ULSS_Helper.Modules;

public class ButtonManager
{
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        try
        {
            //RPH log reader buttons
            if (e.Id is "send" or "send2") await RphLogAnalysisMessages.SendMessageToUser(e);
            if (e.Id == "info") await RphLogAnalysisMessages.SendDetailedInfoMessage(e);
        
            //ELS log reader buttons
            if (e.Id is "sendElsToUser" or "sendElsDetailsToUser") await ElsLogAnalysisMessages.SendMessageToUser(e);
            if (e.Id == "elsDetails") await ElsLogAnalysisMessages.SendDetailedInfoMessage(e);
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}