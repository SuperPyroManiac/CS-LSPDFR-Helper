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
        [Autocomplete(typeof(PluginAutoComplete)),Option("Name", "Must match an existing plugin name!")] string pluginName)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var plugin in Database.LoadPlugins())
        {
            if (plugin.Name == pluginName)
            {
                await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));
                isValid = true;
                await ctx.CreateResponseAsync(bd.AddEmbed(
                    BasicEmbeds.Success($"**Removed plugin: {pluginName}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, 
                    BasicEmbeds.Warning($"Removed plugin: {pluginName}!\r\n>>> " +
                                        $"**Display Name:** {plugin.DName}\r\n" +
                                        $"**Version:** {plugin.Version}\r\n" +
                                        $"**EA Version:** {plugin.EAVersion}\r\n" +
                                        $"**ID:** {plugin.ID}\r\n" +
                                        $"**Link:** {plugin.Link}\r\n" +
                                        $"**Notes:**\r\n" +
                                        $"```{plugin.Description}```\r\n" +
                                        $"**State:** {plugin.State}", true));
                Database.DeletePlugin(plugin);
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