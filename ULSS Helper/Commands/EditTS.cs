using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditTs : ApplicationCommandModule
{
    [SlashCommand("ChangeErrorView", "Edits what you see in more details!")]
    [RequireTsRoleSlash]
    public async Task EditViewCmd(
        InteractionContext ctx, 
        [Option("View", "True shows XTRA errors, False does not.")] bool view)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadTs().All(ts => ts.ID.ToString() != ctx.Member.Id.ToString()))
        {
            var botAdminMentions = Program.Settings.Env.BotAdminUserIds.Select(botAdminId => $"<@{botAdminId}>").ToList();
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"You are not in the DB, please contact {string.Join(" or ", botAdminMentions)}!")));
            return;
        }

        var viewint = 1;
        if (!view) viewint = 0;
        
        var ts = Database.LoadTs().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
        ts!.View = viewint;
        ts.Username = ctx.Guild.GetMemberAsync(ulong.Parse(ts.ID)).Result.Username;
        
        Database.EditTs(ts);
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"<@{ctx.Member.Id.ToString()}>: You have changed your view type to: {(view ? "Show XTRA Errors" : "Hide XTRA Errors")}")));
        Logging.SendLog(
            ctx.Interaction.Channel.Id, 
            ctx.Interaction.User.Id,
            BasicEmbeds.Info($"**<@{ctx.Member.Id.ToString()}> has changed their view type to: {(view ? "Show XTRA errors" : "Hide XTRA Errors")}**")
        );
    }
    
    [SlashCommand("AllowPerms", "Allows a TS to use commands!")]
    [RequireBotAdmin]
    public async Task EditAllowCmd(
        InteractionContext ctx, 
        [Option("ID", "User ID to change!")] string userId,
        [Option("Allow", "Allow access to advanced bot commands!")] bool allow)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Database.LoadTs().All(x => x.ID.ToString() != userId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("User is not in the DB!")));
            return;
        }
        var allowint = 0;
        if (allow) allowint = 1;

        var ts = Database.LoadTs().FirstOrDefault(x => x.ID.ToString() == userId);
        ts!.Allow = allowint;
        ts.Username = ctx.Guild.GetMemberAsync(ulong.Parse(ts.ID)).Result.Username;
        
        Database.EditTs(ts);
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"<@{userId}>'s advanced command perms have been set to: {(allow ? "Allow" : "Deny")}")));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"<@{userId}>'s advanced command perms have been set to: {(allow ? "Allow" : "Deny")}"));
    }
}