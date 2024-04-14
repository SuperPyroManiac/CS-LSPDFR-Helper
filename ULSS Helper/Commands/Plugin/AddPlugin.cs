using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class AddPlugin : ApplicationCommandModule
{
    [SlashCommand("AddPlugin", "Adds a plugin to the database!")]
    [RequireAdvancedTsRole]
    public async Task AddPluginCmd(
        InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string pluginName, 
        [Option("State", "Plugin state")] State pluginState)
    {
        if (Database.LoadPlugins().Any(plugin => plugin.Name == pluginName))
        {
            var err = new DiscordInteractionResponseBuilder();
            err.IsEphemeral = true;
            await ctx.CreateResponseAsync(err.AddEmbed(BasicEmbeds.Error("This plugin already exists!\r\nConsider using /EditPlugin <Name> <State>", true)));
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
            new DiscordSelectComponentOption("Display Name", "Plugin DName"),
            new DiscordSelectComponentOption("Version", "Plugin Version"),
            new DiscordSelectComponentOption("EA Version", "Plugin EAVersion"),
            new DiscordSelectComponentOption("ID", "Plugin ID"),
            new DiscordSelectComponentOption("Link", "Plugin Link"),
            new DiscordSelectComponentOption("Notes", "Plugin Notes"),
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
                ButtonStyle.Success,
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
        catch (Exception e)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, ComponentInteraction.SelectPluginValueToEdit, new UserActionCache(ctx.Interaction, plugin, msg));
    }
}