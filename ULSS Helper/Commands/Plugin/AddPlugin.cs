using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class AddPlugin
{
    [Command("AddPlugin")]
    [Description("Adds a plugin to the database!")]
    public async Task AddPluginCmd(SlashCommandContext ctx, 
        [Description("Plugins name as shown in the log!")] string pluginName, 
        [Description("Plugin state")] State pluginState)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        if (Database.LoadPlugins().Any(plugin => plugin.Name == pluginName))
        {
            var err = new DiscordInteractionResponseBuilder();
            err.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                err.AddEmbed(BasicEmbeds.Error("__This plugin already exists!__\r\nConsider using /EditPlugin <Name> <State>", true)));
            return;
        }

        Objects.Plugin plugin = new()
        {
            Name = pluginName,
            DName = pluginName,
            Description = "N/A",
            State = pluginState.ToString().ToUpper()
        };
        
        var pluginValues = new List<DiscordSelectComponentOption>()
        {
            new("Display Name", "Plugin DName"),
            new("Version", "Plugin Version"),
            new("EA Version", "Plugin EAVersion"),
            new("ID", "Plugin ID"),
            new("Link", "Plugin Link"),
            new("Notes", "Plugin Notes"),
        };
        
        var embed = BasicEmbeds.Info(
            $"__Adding New Plugin: {plugin.Name}__\r\n>>> " +
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
        var bd = new DiscordInteractionResponseBuilder();
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