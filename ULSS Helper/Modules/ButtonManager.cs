using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Modules;

public class ButtonManager
{
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id is "send" or "send2") await Messages.RphLogAnalysisMessages.SendMessageToUser(e);
        if (e.Id == "info") await Messages.RphLogAnalysisMessages.SendDetailedInfoMessage(e);
    }
}