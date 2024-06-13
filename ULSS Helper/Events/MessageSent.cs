using DSharpPlus;
using DSharpPlus.EventArgs;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Events;

internal class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        if (ctx.Channel.IsPrivate) return;
        
        //Activate PublicSupport MessageSentEvent
        await Modules.Functions.PublicSupportManager.MessageSentEvent(s, ctx);
        
        //Activate AutoHelper MessageSentEvent
        await Public.AutoHelper.MessageSent.MessageSentEvent(s, ctx);

    }
}