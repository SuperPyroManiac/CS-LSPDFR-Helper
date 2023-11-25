using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class PluginNotes : ApplicationCommandModule
{
	[SlashCommand("PluginNotes", "Views / Edits plugin notes!")]

	public async Task PluginNotesCmd(InteractionContext ctx, [Option("Name", "Plugins name as shown in the log!")] string pluginName)
	{
		var bd = new DiscordInteractionResponseBuilder();
		bd.IsEphemeral = true;
		if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
		{
			await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
			return;
		}
		var ts = Database.LoadTs().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
		if (ts == null || ts.Allow == 0)
		{
			await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
			Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
				BasicEmbeds.Warning("**TS attempted to edit plugin notes without permission.**"));
			return;
		}

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