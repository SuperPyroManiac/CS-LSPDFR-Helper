using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class ForceUpdateCheck : ApplicationCommandModule
{
    [SlashCommand("ForceUpdateCheck", "Forced the database to update!")]

    public async Task ForceUpdateCmd(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            return;
        }
        var ts = Database.LoadTS().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
        if (ts == null || ts.Allow == 0)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
                BasicEmbeds.Warning($"**TS attempted to force updates without permission.**"));
            return;
        }

        var th = new Thread(Database.UpdatePluginVersions);
        th.Start();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Info("All plugin versions will be updated!")));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("Forced DB updater to run!"));
    }
}