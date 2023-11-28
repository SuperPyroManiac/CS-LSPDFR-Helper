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
/// Checks whether the user has the TS role.
/// </summary>
public class RequireTsRoleAttribute : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
    {
        if (ctx.Member.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
            return true;
        else 
            await PermissionMessages.SendNoPermissionError(ctx);
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
        bool hasTsRole = ctx.Member.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId);
        bool IsWhitelistedForCommands = false;

        var ts = Database.LoadTs().FirstOrDefault(ts => ts.ID.ToString() == ctx.Member.Id.ToString());
        if (ts != null && ts.Allow == 0)
            Logging.SendLog(
                ctx.Interaction.Channel.Id, 
                ctx.Interaction.User.Id,
                BasicEmbeds.Warning($"**TS attempted to use an advanced command without permission!**\r\nCommand name: {ctx.CommandName}")
            );
        else 
            IsWhitelistedForCommands = ts == null ? false : true;
        
        if (hasTsRole && IsWhitelistedForCommands)
            return true;
        else
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
        if (Program.Settings.Env.BotAdminUserIds.Any(adminId => adminId == ctx.Member.Id))
            return true;
        else
        {
            await PermissionMessages.SendNoPermissionError(ctx);
            return false;
        }
    }
}
