using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddPlugin : ApplicationCommandModule
{
    [SlashCommand("AddPlugin", "Adds a plugin to the database!")]
    [RequireAdvancedTsRole()]
    public async Task AddPluginCmd(
        InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string pluginName, 
        [Option("State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB")] State pluginState)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        if (Database.LoadPlugins().Any(plugin => plugin.Name == pluginName))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("This plugin already exists in the database!\r\nConsider using /EditPlugin <Name> <State>")));
            return;
        }

        Plugin plugin = new()
        {
            Name = pluginName,
            State = pluginState.ToString().ToUpper()
        };
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId("add-plugin");
        modal.WithTitle($"Adding {plugin.Name} as {plugin.State}");
        modal.AddComponents(new TextInputComponent(
            label: "Display Name:", 
            customId: "plugDName", 
            required: true, 
            style: TextInputStyle.Short, 
            value: plugin.Name
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Version:",
            customId: "plugVersion",
            required: false,
            style: TextInputStyle.Short
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Early Access Version:",
            customId: "plugEAVersion",
            required: false,
            style: TextInputStyle.Short
        ));
        modal.AddComponents(new TextInputComponent(
            label: "ID (on lcpdfr.com):",
            customId: "plugID",
            required: false,
            style: TextInputStyle.Short
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Link:",
            customId: "plugLink",
            required: false,
            style: TextInputStyle.Short
        ));
        
		Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, plugin));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}