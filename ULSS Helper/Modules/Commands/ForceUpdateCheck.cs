using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace ULSS_Helper.Modules.Commands;

public class ForceUpdateCheck : ApplicationCommandModule
{
    [SlashCommand("ForceUpdateCheck", "Forced the database to update!")]
    [SlashRequirePermissions(Permissions.ManageMessages)]
    public async Task ForceUpdateCmd(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        var th = new Thread(DatabaseManager.UpdatePluginVersions);
        th.Start();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Info("All plugin versions will be updated!")));
    }
}