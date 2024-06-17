using DSharpPlus;
using DSharpPlus.EventArgs;

namespace LSPDFR_Helper.EventManagers;

internal static class CompInteraction
{
    private static readonly List<string> CacheEventIds =
    [
        CustomIds.SelectAttachmentForAnalysis,
        CustomIds.SelectIdForRemoval,
        CustomIds.SelectPluginValueToEdit,
        CustomIds.SelectPluginValueToFinish,
        CustomIds.SelectErrorValueToEdit,
        CustomIds.SelectErrorValueToFinish,
        CustomIds.RphGetQuickInfo,
        CustomIds.RphGetDetailedInfo,
        CustomIds.RphGetAdvancedInfo,
        CustomIds.RphQuickSendToUser,
        CustomIds.RphDetailedSendToUser,
        CustomIds.ElsGetQuickInfo,
        CustomIds.ElsGetDetailedInfo,
        CustomIds.ElsQuickSendToUser,
        CustomIds.ElsDetailedSendToUser,
        CustomIds.AsiGetQuickInfo,
        CustomIds.AsiGetDetailedInfo,
        CustomIds.AsiQuickSendToUser,
        CustomIds.AsiDetailedSendToUser,
        CustomIds.ShvdnGetQuickInfo,
        CustomIds.ShvdnGetDetailedInfo,
        CustomIds.ShvdnQuickSendToUser,
        CustomIds.ShvdnDetailedSendToUser
    ];
    
    internal static async Task HandleInteraction(DiscordClient s, ComponentInteractionCreatedEventArgs eventArgs)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);

        if ( CacheEventIds.Any(eventId => eventId == eventArgs.Id) )
        {//Handle cached interaction events here.
            //TODO: var cache = Program.Cache.GetProcess(eventArgs.Message.Id);
        }
        //Handle non cached interaction events here.
    }
}