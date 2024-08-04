using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Global;

public enum Choice
{
    PLUGINS,
    USERS,
    CASES,
    SERVERS,
    CACHE,
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
        int closed;
        int servers;
        switch ( choice )
        {
            case Choice.PLUGINS:
                _ = Task.Run(Plugins.UpdateVersions);
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced Plugin Verification!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced Plugin Verification!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish."));
                break;
            case Choice.USERS:
                missing = await Users.Missing();
                usernames = await Users.Usernames();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced User Verification!__\r\n>>> User count: {Program.Cache.GetUsers().Count}\r\nMissing users added: {missing}\r\nUsernames updated: {usernames}")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced User Verification!__\r\n>>> User count: {Program.Cache.GetUsers().Count}\r\nMissing users added: {missing}\r\nUsernames updated: {usernames}"));
                break;
            case Choice.CASES:
                closed = await AutoHelper.ValidateClosedCases();
                closed += await AutoHelper.ValidateOpenCases();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced Case Verification!__\r\n>>> Case count: {Program.Cache.GetCases().Count}\r\nClosed: {closed} cases.")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced Case Verification!__\r\n>>> Case count: {Program.Cache.GetCases().Count}\r\nClosed: {closed} cases."));
                break;
            case Choice.SERVERS:
                servers = await Servers.AddMissing();
                servers += await Servers.RemoveMissing();
                await Servers.Validate();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced Server Verification!__\r\n>>> Server Count: {Program.Cache.ServerCacheDict.Count}\r\nModified: {servers} servers.")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced Server Verification!__\r\n>>> Server count: {Program.Cache.ServerCacheDict.Count}\r\nModified: {servers} servers."));
                break;
            case Choice.CACHE:
                Program.Cache.ResetCaches();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success("__Forced Cache Update!__\r\n>>> All caches have been cleared and reset!")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Forced Cache Update!__\r\n>>> All caches have been cleared and reset!"));
                break;
            case Choice.ALL:
                _ = Task.Run(Plugins.UpdateVersions);
                missing = await Users.Missing();
                usernames = await Users.Usernames();
                closed = await AutoHelper.ValidateClosedCases();
                closed += await AutoHelper.ValidateOpenCases();
                servers = await Servers.AddMissing();
                servers += await Servers.RemoveMissing();
                await Servers.Validate();
                Program.Cache.ResetCaches();
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    bd.AddEmbed(BasicEmbeds.Success($"__Forced All Verifications!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.\r\nUser count: {Program.Cache.GetUsers().Count}\r\nMissing users added: {missing}\r\nUsernames updated: {usernames}\r\nCase count: {Program.Cache.GetCases().Count}\r\nClosed: {closed} cases.\r\nServer count: {Program.Cache.ServerCacheDict.Count}\r\nModified: {servers} servers.")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__Forced All Verifications!__\r\n>>> Plugin count: {Program.Cache.GetPlugins().Count}\r\nAllow 10 - 15 minutes for updater to finish.\r\nUser count: {Program.Cache.GetUsers().Count}\r\nMissing users added: {missing}\r\nUsernames updated: {usernames}\r\nCase count: {Program.Cache.GetCases().Count}\r\nClosed: {closed} cases.\r\nServer count: {Program.Cache.ServerCacheDict.Count}\r\nModified: {servers} servers."));
                break;
        }
    }
}