using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditPlugin : ApplicationCommandModule
{
    [SlashCommand("EditPlugin", "Edits a plugin in the database!")]
    [RequireAdvancedTsRole]
    public async Task EditPluginCmd
    (
        InteractionContext ctx, 
        [Autocomplete(typeof(PluginAutoComplete)),Option("Name", "Plugins name as shown in the log!")] string pluginName, 
        [Option("New_State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB, IGNORE")] State? newState=null
    )
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        if (Database.LoadPlugins().All(x => x.Name != pluginName))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pluginName}")));
            return;
        }

        var plugin = Database.GetPlugin(pluginName);

        if (newState != null)
        {
            plugin.State = newState.ToString()!.ToUpper();
        }
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId(ModalSubmit.EditPlugin);
        modal.WithTitle($"Editing {plugin.Name} as {plugin.State}");
        modal.AddComponents(new TextInputComponent(
            label: "Display Name:", 
            customId: "plugDName", 
            required: true, 
            style: TextInputStyle.Short, 
            value: plugin.DName
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Version:", 
            customId: "plugVersion", 
            required: false,
            style: TextInputStyle.Short, 
            value: plugin.Version
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Early Access Version:", 
            customId: "plugEAVersion", 
            required: false,
            style: TextInputStyle.Short, 
            value: plugin.EAVersion
        ));
        modal.AddComponents(new TextInputComponent(
            label: "ID (on lcpdfr.com):", 
            customId: "plugID", 
            required: false, 
            style: TextInputStyle.Short, 
            value: plugin.ID
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Link:", 
            customId: "plugLink", 
            required: false, 
            style: TextInputStyle.Short, 
            value: plugin.Link
        ));
		Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, plugin));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}