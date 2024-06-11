using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Modules.Functions;

public static class PermissionManager
{
    private static async Task SendNoPermissionError(SlashCommandContext ctx)
    {
        await ctx.Interaction.DeferAsync(true);
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.EditResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error($"__Permission Denied!__\r\n>>> You cannot use: `{ctx.Command.Name}`", true)));
    }
    
    private static async Task SendBlacklistPermissionError(SlashCommandContext ctx)
    {
        await ctx.Interaction.DeferAsync(true);
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.EditResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error($"__You are blacklisted!__\r\n>>> Contact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!", true)));
    }
    
    private static async Task SendNoAdvancedPermissionError(SlashCommandContext ctx)
    {
        await Logging.SendLog(
            ctx.Channel.Id, 
            ctx.User.Id,
            BasicEmbeds.Warning($"__TS attempted to use an advanced command!__\r\n>>> Command name: {ctx.Command.Name}", true)
        );
    }
    
    /// <summary>
    /// Checks whether the user has the TS role for commands.
    /// </summary>
    public static async Task<bool> RequireTs(SlashCommandContext ctx)
    {
        if (await Program.Cache.GetUser(ctx.User.Id.ToString()).IsTs()) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user has the TS role and is allowed to use advanced commands.
    /// </summary>
    public static async Task<bool> RequireAdvancedTs(SlashCommandContext ctx)
    {
        var isWhitelistedForCommands = false;
        var ts = Program.Cache.GetUser(ctx.User.Id.ToString());
        if (ts != null && ts.BotEditor == 0) await SendNoAdvancedPermissionError(ctx);
        else 
            isWhitelistedForCommands = ts != null;
        
        if (await Program.Cache.GetUser(ctx.User.Id.ToString()).IsTs() && isWhitelistedForCommands)
            return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user is part of the list of bot admins.
    /// </summary>
    public static async Task<bool> RequireBotAdmin(SlashCommandContext ctx)
    {
        if (Convert.ToBoolean(Program.Cache.GetUser(ctx.User.Id.ToString()).BotAdmin)) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    
    /// <summary>
    /// Checks whether the user is blocked from public usage.
    /// </summary>
    public static async Task<bool> RequireNotBlacklisted(SlashCommandContext ctx)
    {
        if (!Convert.ToBoolean(Program.Cache.GetUser(ctx.User.Id.ToString()).Blocked)) return true;
        await SendBlacklistPermissionError(ctx);
        return false;
    }
}
