using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.EventManagers;

internal static class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        if ( ctx.Channel.IsPrivate ) return;

        await AutoReplies.MonitorMessages(ctx);
    }
}