using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Plugin;

public class EditPlugin
{
    [Command("editplugin")]
    [Description("Edits a plugin in the database!")]
    public async Task EditPluginCmd
    (
        SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName,
        [Description("Plugin type.")] PluginType newtype = default,
        [Description("Plugin state.")] State newstate = default
    )
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (DbManager.GetPlugins().All(x => x.Name != pluginName))
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pluginName}")));
            return;
        }

        var plugin = DbManager.GetPlugin(pluginName);

        if (newstate != default) plugin.State = newstate;
        if (newtype != default) plugin.PluginType = newtype;
        
        var pluginValues = new List<DiscordSelectComponentOption>
        {
            new("Display Name", "Plugin DName"),
            new("Version", "Plugin Version"),
            new("Ea Version", "Plugin EaVersion"),
            new("Id", "Plugin Id"),
            new("Link", "Plugin Link"),
            new("Notes", "Plugin Notes"),
            new("Author Id", "Plugin AuthorId"),
            new("Announce", "Plugin Announce")
        };
        
        var embed = BasicEmbeds.Info(
            $"__Editing Plugin: {plugin.Name}__\r\n>>> " +
            $"**Display Name:** {plugin.DName}\r\n" +
            $"**Version:** {plugin.Version}\r\n" +
            $"**Ea Version:** {plugin.EaVersion}\r\n" +
            $"**Id:** {plugin.Id}\r\n" +
            $"**Link:** {plugin.Link}\r\n" +
            $"**Author Id:** {plugin.AuthorId}\r\n" +
            $"**Announce:** {plugin.Announce}\r\n" +
            $"**Notes:**\r\n" +
            $"```{plugin.Description}```\r\n" +
            $"**Type:** {plugin.PluginType}\r\n" +
            $"**State:** {plugin.State}");
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectPluginValueToEdit,
                placeholder: "Edit Value",
                options: pluginValues
            ));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                CustomIds.SelectPluginValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":yes:"))));
        
        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit, new InteractionCache(ctx.Interaction, plugin, msg));
    }
}