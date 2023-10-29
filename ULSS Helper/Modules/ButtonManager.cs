using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Modules;

public class ButtonManager
{
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        //Select log buttons
        //TODO: make the dang thing
        
        //RPH log reader buttons
        if (e.Id is "send" or "send2") await Messages.RphLogAnalysisMessages.SendMessageToUser(e);
        if (e.Id == "info") await Messages.RphLogAnalysisMessages.SendDetailedInfoMessage(e);
        
        //ELS log reader buttons
        //TODO: make the dang thing
    }
}