using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Events;

public class MessageSent
{
    public static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        while (!Program.isStarted) await Task.Delay(500);
        if (ctx.Channel.IsPrivate) return;
        
        //Activate PublicSupport MessageSentEvent
        await Modules.Functions.PublicSupportManager.MessageSentEvent(s, ctx);
        
        //Activate AutoHelper MessageSentEvent
        await Public.AutoHelper.MessageSent.MessageSentEvent(s, ctx);

    }
}