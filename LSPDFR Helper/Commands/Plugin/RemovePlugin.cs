using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.AutoCompleteTypes;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Plugin;

public class RemovePlugin
{
    [Command("removeplugin")]
    [Description("Removes a plugin from the database!")]
    public async Task RemovePluginCmd(SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var plugin in DbManager.GetPlugins())
        {
            if (plugin.Name == pluginName)
            {
                isValid = true;
                await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd.AddEmbed(
                    BasicEmbeds.Success($"**Removed plugin: {pluginName}**")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, 
                    BasicEmbeds.Warning($"__Removed plugin: {pluginName}!__\r\n>>> " +
                                        $"**Display Name:** {plugin.DName}\r\n" +
                                        $"**Version:** {plugin.Version}\r\n" +
                                        $"**Ea Version:** {plugin.EaVersion}\r\n" +
                                        $"**Id:** {plugin.Id}\r\n" +
                                        $"**Link:** {plugin.Link}\r\n" +
                                        $"**Notes:**\r\n" +
                                        $"```{plugin.Description}```\r\n" +
                                        $"**Type:** {plugin.PluginType}\r\n" +
                                        $"**State:** {plugin.State}"));
                DbManager.DeletePlugin(plugin);
                Program.Cache.UpdatePlugins(DbManager.GetPlugins());
                return;
            }
        }
        if (!isValid)
        {
            await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"**No plugin found with name: {pluginName}!**")));
        }
    }
}