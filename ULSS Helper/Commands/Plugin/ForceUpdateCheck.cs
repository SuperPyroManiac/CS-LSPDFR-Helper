using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;

namespace ULSS_Helper.Commands.Plugin;

public class ForceUpdateCheck
{
    [Command("ForceUpdateCheck")]
    [Description("Forced the database to update!")]
    public async Task ForceUpdateCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            bd.AddEmbed(BasicEmbeds.Success("__Started DB updater!__\r\nPlease allow a few minutes for everything to update!", true)));
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Forced DB updater to run!__", true));
    }
}