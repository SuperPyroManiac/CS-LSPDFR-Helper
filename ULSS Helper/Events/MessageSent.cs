using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Events;

internal class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        var dbUsers = Database.LoadUsers();
        //Bully
        if (dbUsers.Any(x => x.UID == ctx.Author.Id.ToString() && x.Bully == 1))
        {
            var rNd = new Random().Next(4);
            if (rNd == 1) await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
            if (rNd == 2) await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":middle_finger:"));
            if (rNd == 0)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":tarabruh:"));
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(s, ":middle_finger:"));
            }
        }
        
        //Add Users
        if (dbUsers.All(x => x.UID.ToString() != ctx.Author.Id.ToString()))
        {
            var newUser = new DiscordUser()
            {
                UID = ctx.Author.Id.ToString(),
                Username = ctx.Author.Username,
                BotEditor = 0,
                BotAdmin = 0,
                Bully = 0,
                Blocked = 0
            };
            Database.AddUser(newUser);
        }
        
        //Activate Public MessageSentEvent
        await Public.AutoHelper.MessageSent.MessageSentEvent(s, ctx);
        // var th = new Thread(() => Public.AutoHelper.MessageSent.MessageSentEvent(s, ctx).GetAwaiter());
        // th.Start();
    }
}