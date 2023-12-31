using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Events;

internal class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        if (Program.Settings.Env.BullyingVictims.Any(victimId => victimId == ctx.Author.Id))
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
        
        //Activate Public MessageSentEvent
        await Public.Events.MessageSent.MessageSentEvent(s, ctx);
    }
}