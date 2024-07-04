using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.EventManagers;

public static class MessageSent
{
    public static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        if ( ctx.Channel.IsPrivate ) return;

        await AutoReplies.MonitorMessages(ctx);
        _ = Task.Run(() => Functions.AutoHelper.MessageMonitor.MessageSentEvent(s, ctx));
    }
}