using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class BlacklistUser : ApplicationCommandModule
{
    [SlashCommand("BlacklistUser", "Blocks a user from using the bot!")]
    [RequireBotAdmin]
    public async Task BlacklistUserCmd(
        InteractionContext ctx, 
        [Option("ID", "User ID to toggle!")] string userId)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadUsers().All(x => x.UID.ToString() != userId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("User is not in the DB!")));
            return;
        }

        var dUser = Database.LoadUsers().FirstOrDefault(x => x.UID.ToString() == userId);
        if (dUser.Blocked == 0) dUser.Blocked = 1;
        if (dUser.Blocked == 1) dUser.Blocked = 0;
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"<@{userId}>'s blocked status: {dUser.Blocked}")));
        Logging.SendLog(
            ctx.Interaction.Channel.Id,
            ctx.Interaction.User.Id,
            BasicEmbeds.Info(
                $"User <@{dUser.UID}> ({dUser.Username}) has had their blacklist status changed!\r\nBlocked: {dUser.Blocked}", true));
    }
}