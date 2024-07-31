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
        await ctx.EditResponseAsync(responseBuilder.AddEmbed(BasicEmbeds.Error($"__You are blacklisted from the bot!__\r\n>>> Contact the devs at https://dsc.PyrosFun.com if you think this is an error!")));
    }
    
    /// <summary>
    /// Checks whether the user is a server manager.
    /// </summary>
    public static async Task<bool> RequireServerManager(SlashCommandContext ctx)
    {
        if ( Program.Cache.GetUser(ctx.User.Id).BotAdmin ) return true;
        if ((ctx.Member!.Permissions & DiscordPermissions.Administrator) != 0) return true;
        if (await Program.Cache.GetUser(ctx.User.Id).IsManager(ctx.Guild!.Id)) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user is a server admin.
    /// </summary>
    public static async Task<bool> RequireServerAdmin(SlashCommandContext ctx)
    {
        if ( Program.Cache.GetUser(ctx.User.Id).BotAdmin ) return true;
        if ((ctx.Member!.Permissions & DiscordPermissions.Administrator) != 0) return true;
        await SendNoPermissionError(ctx);
        return false;
    }
    
    /// <summary>
    /// Checks whether the user is a bot editor.
    /// </summary>
    public static async Task<bool> RequireBotEditor(SlashCommandContext ctx)
    {
        if ( Program.Cache.GetUser(ctx.User.Id).BotAdmin ) return true;
        if (Program.Cache.GetUser(ctx.User.Id).BotEditor) return true;
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
