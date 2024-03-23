using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class ForceUpdateCheck : ApplicationCommandModule
{
    [SlashCommand("ForceUpdateCheck", "Forced the database to update!")]
    [RequireAdvancedTsRole]
    public async Task ForceUpdateCmd(InteractionContext ctx)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success("Started DB updater!\r\nPlease allow up to 2 minutes for everything to update!", true)));
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("Forced DB updater to run!", true));
    }
}