using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Plugin;

public class RemovePlugin
{
    [Command("removeplugin")]
    [Description("Removes a plugin from the database!")]
    public async Task RemovePluginCmd(SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
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