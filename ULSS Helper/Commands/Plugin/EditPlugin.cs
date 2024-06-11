using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class EditPlugin
{
    [Command("EditPlugin")]
    [Description("Edits a plugin in the database!")]
    public async Task EditPluginCmd
    (
        SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName, 
        [Description("Plugin state.")] State? newState = null
    )
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (Database.LoadPlugins().All(x => x.Name != pluginName))
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pluginName}")));
            return;
        }

        var plugin = Database.GetPlugin(pluginName);

        if (newState != null) plugin.State = newState.ToString()!.ToUpper();
        
        var pluginValues = new List<DiscordSelectComponentOption>()
        {
            new DiscordSelectComponentOption("Display Name", "Plugin DName"),
            new DiscordSelectComponentOption("Version", "Plugin Version"),
            new DiscordSelectComponentOption("EA Version", "Plugin EAVersion"),
            new DiscordSelectComponentOption("ID", "Plugin ID"),
            new DiscordSelectComponentOption("Link", "Plugin Link"),
            new DiscordSelectComponentOption("Notes", "Plugin Notes"),
        };
        
        var embed = BasicEmbeds.Info(
            $"__Editing Plugin: {plugin.Name}__\r\n>>> " +
            $"**Display Name:** {plugin.DName}\r\n" +
            $"**Version:** {plugin.Version}\r\n" +
            $"**EA Version:** {plugin.EAVersion}\r\n" +
            $"**ID:** {plugin.ID}\r\n" +
            $"**Link:** {plugin.Link}\r\n" +
            $"**Notes:**\r\n" +
            $"```{plugin.Description}```\r\n" +
            $"**State:** {plugin.State}", true);
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: ComponentInteraction.SelectPluginValueToEdit,
                placeholder: "Edit Value",
                options: pluginValues
            ));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                ComponentInteraction.SelectPluginValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":yes:"))));
        
        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, ComponentInteraction.SelectPluginValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, ComponentInteraction.SelectPluginValueToEdit, new UserActionCache(ctx.Interaction, plugin, msg));
    }
}