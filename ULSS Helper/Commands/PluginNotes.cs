using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class PluginNotes : ApplicationCommandModule
{
	[SlashCommand("PluginNotes", "Views / Edits plugin notes!")]
	[RequireAdvancedTsRole]
	public async Task PluginNotesCmd
	(
		InteractionContext ctx, 
		[Autocomplete(typeof(PluginAutoComplete)),Option("Name", "Plugins name as shown in the log!")] string pluginName
	)
	{
		var bd = new DiscordInteractionResponseBuilder();
		bd.IsEphemeral = true;

		if (Database.LoadPlugins().All(plugin => plugin.Name != pluginName))
		{
			await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pluginName}")));
			return;
		}

		var plugin = Database.LoadPlugins().FirstOrDefault(x => x.Name == pluginName);

		DiscordInteractionResponseBuilder modal = new();
		modal.WithCustomId("edit-pluginnotes");
		modal.WithTitle($"Editing {plugin.Name}'s notes!");
		modal.AddComponents(new TextInputComponent(
			label: "Notes:", 
			customId: "plugnotes", 
			required: true, 
			style: TextInputStyle.Paragraph, 
			value: plugin.Description
		));

		Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new(ctx.Interaction, plugin));	
		await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);	
	}
}