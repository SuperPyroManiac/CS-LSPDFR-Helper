using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Global;

public enum Choice
{
    PLUGINS,
    USERS,
    CASES,
    ALL
}

public class ForceVerification
{
    [Command("forceverification")]
    [Description("Forced the database to update!")]
    public async Task ForceUpdateCmd(SlashCommandContext ctx, [Description("Plugin state")] Choice choice)
    {
        if (!await PermissionManager.RequireBotAdmin(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        int missing;
        int usernames;
        switch ( choice )
        {
            case Choice.PLUGINS:
                Plugins.UpdateVersions();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced Plugin Verification!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.", true)));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced Plugin Verification!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.", true));
                break;
            case Choice.USERS:
                missing = await Users.Missing();
                usernames = await Users.Usernames();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Verifying all users!__\r\n>>> User count: {Program.Cache.GetUsers().Count}\r\nMissing users added: {missing}\r\nUsernames updated: {usernames}", true)));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced Plugin Verification!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.", true));
                break;
            case Choice.CASES:
                break;
            case Choice.ALL:
                Plugins.UpdateVersions();
                missing = await Users.Missing();
                usernames = await Users.Usernames();
                break;
        }
    }
}