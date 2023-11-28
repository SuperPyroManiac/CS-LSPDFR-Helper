using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class RemovePlugin : ApplicationCommandModule
{
    [SlashCommand("RemovePlugin", "Removes a plugin from the database!")]
    [RequireAdvancedTsRole]
    public async Task RemovePluginCmd(InteractionContext ctx,
        [Option("Name", "Must match an existing plugin name!")] string pluginName)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var plugin in Database.LoadPlugins())
        {
            if (plugin.Name == pluginName)
            {
                Database.DeletePlugin(plugin);
                isValid = true;
                await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"**Removed: {pluginName}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning($"Removed plugin: {pluginName}!"));
                return;
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!isValid)
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"**No plugin found with name: {pluginName}!**")));
        }
    }
}