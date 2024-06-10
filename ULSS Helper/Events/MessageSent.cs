using DSharpPlus;
using DSharpPlus.EventArgs;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Events;

internal class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        if (ctx.Channel.IsPrivate) return;
        
        //Add Users
        if (Program.Cache.GetUsers().All(x => x.UID.ToString() != ctx.Author.Id.ToString()))
        {
            var newUser = new DiscordUser()
            {
                UID = ctx.Author.Id.ToString(),
                Username = ctx.Author.Username,
                BotEditor = 0,
                BotAdmin = 0,
                Blocked = 0
            };
            Database.AddUser(newUser);
        }
        
        //Activate PublicSupport MessageSentEvent
        await Modules.Functions.PublicSupportManager.MessageSentEvent(s, ctx);
        
        //Activate AutoHelper MessageSentEvent
        await Public.AutoHelper.MessageSent.MessageSentEvent(s, ctx);

    }
}