using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class RemovePlugin
{
    [Command("RemovePlugin")]
    [Description("Removes a plugin from the database!")]
    public async Task RemovePluginCmd(SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var plugin in Database.LoadPlugins())
        {
            if (plugin.Name == pluginName)
            {
                isValid = true;
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd.AddEmbed(
                    BasicEmbeds.Success($"**Removed plugin: {pluginName}**")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, 
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
            await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"**No plugin found with name: {pluginName}!**")));
        }
    }
}