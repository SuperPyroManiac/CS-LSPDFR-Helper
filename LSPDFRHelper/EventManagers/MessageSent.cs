using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.EventManagers;

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