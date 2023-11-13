using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class RemovePlugin : ApplicationCommandModule
{
    [SlashCommand("RemovePlugin", "Removes a plugin from the database!")]

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
        foreach (var plugin in Database.LoadPlugins())
        {
            if (plugin.Name == plugName)
            {
                Database.DeletePlugin(plugin);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning($"**Removed: {plugName}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning($"Removed plugin: {plugName}!"));
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