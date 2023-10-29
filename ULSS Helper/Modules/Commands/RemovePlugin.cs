using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using ULSS_Helper.Modules.Messages;

namespace ULSS_Helper.Modules.Commands;

public class RemovePlugin : ApplicationCommandModule
{
    [SlashCommand("RemovePlugin", "Removes a plugin from the database!")]
    [SlashRequirePermissions(Permissions.ManageMessages)]
    public async Task RemovePluginCmd(InteractionContext ctx,
        [Option("Name", "Must match an existing plugin name!")] string plugName)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        
        var isValid = false;
        foreach (var plugin in DatabaseManager.LoadPlugins())
        {
            if (plugin.Name == plugName)
            {
                DatabaseManager.DeletePlugin(plugin);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning($"**Removed: {plugName}**")));
                return;
            }
        }
        if (!isValid)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error($"**No plugin found with name: {plugName}!**")));
            return;
        }
    }
}