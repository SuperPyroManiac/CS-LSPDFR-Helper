using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class ForceUpdateCheck : ApplicationCommandModule
{
    [SlashCommand("ForceUpdateCheck", "Forced the database to update!")]
    [RequireAdvancedTsRole()]
    public async Task ForceUpdateCmd(InteractionContext ctx)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success("Started DB updater thread!\r\nPlease allow up to 2 minutes for everything to update!")));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("Forced DB updater to run!"));
    }
}