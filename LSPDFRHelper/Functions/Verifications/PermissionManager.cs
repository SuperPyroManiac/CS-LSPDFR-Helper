using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class PermissionManager
{
    private static async Task SendNoPermissionError(SlashCommandContext ctx)
    {
        await ctx.Interaction.DeferAsync(true);
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.EditResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error($"__Permission Denied!__\r\n>>> You cannot use: `{ctx.Command.Name}`")));
    }
    
    private static async Task SendBlacklistPermissionError(SlashCommandContext ctx)
    {
        await ctx.Interaction.DeferAsync(true);
        var responseBuilder = new DiscordInteractionResponseBuilder { IsEphemeral = true };
        await ctx.EditResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error($"__You are blacklisted!__\r\n>>> Contact server staff in <#{Program.Settings.StaffContactChId}> if you think this is an error!")));
    }
    
    private static async Task SendNoAdvancedPermissionError(SlashCommandContext ctx)
    {
        await Logging.SendLog(
            ctx.Channel.Id, 
            ctx.User.Id,
            BasicEmbeds.Warning($"__TS attempted to use an advanced command!__\r\n>>> Command name: {ctx.Command.Name}")
        );
    }
    
    /// <summary>
    /// Checks whether the user has the TS role for commands.
    /// </summary>
    public static async Task<bool> RequireTs(SlashCommandContext ctx)
    {
        if (await Program.Cache.GetUser(ctx.User.Id).IsTs()) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user has the TS role and is allowed to use advanced commands.
    /// </summary>
    public static async Task<bool> RequireAdvancedTs(SlashCommandContext ctx)
    {
        var isWhitelistedForCommands = false;
        var ts = Program.Cache.GetUser(ctx.User.Id);
        if (ts != null && !ts.BotEditor) await SendNoAdvancedPermissionError(ctx);
        else 
            isWhitelistedForCommands = ts != null;
        
        if (await Program.Cache.GetUser(ctx.User.Id).IsTs() && isWhitelistedForCommands)
            return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user is part of the list of bot admins.
    /// </summary>
    public static async Task<bool> RequireBotAdmin(SlashCommandContext ctx)
    {
        if (Program.Cache.GetUser(ctx.User.Id).BotAdmin) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    
    /// <summary>
    /// Checks whether the user is blocked from public usage.
    /// </summary>
    public static async Task<bool> RequireNotBlacklisted(SlashCommandContext ctx)
    {
        if (!Program.Cache.GetUser(ctx.User.Id).Blocked) return true;
        await SendBlacklistPermissionError(ctx);
        return false;
    }
}
