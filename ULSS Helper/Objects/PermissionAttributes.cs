using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Objects;

internal static class PermissionMessages
{
    internal static async Task SendNoPermissionError(InteractionContext ctx)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.CreateResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
        return;
    }
}

/// <summary>
/// Checks whether the user has the TS role for slash commands.
/// </summary>
public class RequireTsRoleSlash : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.Member.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
            return true;
        await PermissionMessages.SendNoPermissionError(ctx);
        return false;
    }
}

/// <summary>
/// Checks whether the user has the TS role for app commands.
/// </summary>
public class RequireTsRoleContext : ContextMenuCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(ContextMenuContext ctx)
    {
        if (ctx.Member.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
            return true;
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, 
            responseBuilder.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));

        return false;
    }
}

/// <summary>
/// Checks whether the user has the TS role and is allowed to use advanced commands according to our DB.
/// </summary>
public class RequireAdvancedTsRoleAttribute : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        var hasTsRole = ctx.Member.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId);
        var IsWhitelistedForCommands = false;

        var ts = Database.LoadUsers().FirstOrDefault(ts => ts.UID.ToString() == ctx.Member.Id.ToString());
        if (ts != null && ts.BotEditor == 0)
            Logging.SendLog(
                ctx.Interaction.Channel.Id, 
                ctx.Interaction.User.Id,
                BasicEmbeds.Warning($"**TS attempted to use an advanced command without permission!**\r\nCommand name: {ctx.CommandName}")
            );
        else 
            IsWhitelistedForCommands = ts == null ? false : true;
        
        if (hasTsRole && IsWhitelistedForCommands)
            return true;
        await PermissionMessages.SendNoPermissionError(ctx);
        return false;
    }
}

/// <summary>
/// Checks whether the user is part of the list of bot admins.
/// </summary>
public class RequireBotAdminAttribute : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (Database.LoadUsers().Any(x => x.UID == ctx.Member.Id.ToString() && x.BotAdmin == 1))
            return true;
        await PermissionMessages.SendNoPermissionError(ctx);
        return false;
    }
}

/// <summary>
/// Checks whether the user is blocked from public usage.
/// </summary>
public class RequireNotOnBotBlacklist : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (Database.LoadUsers().Any(x => x.UID == ctx.User.Id.ToString() && x.Blocked == 1))
        {
            var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
            responseBuilder.AddEmbed(BasicEmbeds.Error(
                $"You are blacklisted from the bot!\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!"
            ));
            await ctx.CreateResponseAsync(responseBuilder);
            return false;
        }
        return true;
    }
}
