using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class PluginNotes : ApplicationCommandModule
{
	[SlashCommand("PluginNotes", "Views / Edits plugin notes!")]

	public async Task PluginNotesCmd(InteractionContext ctx, [Option("Name", "Plugins name as shown in the log!")] string pN)
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

		if (Database.LoadPlugins().All(x => x.Name != pN))
		{
			await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pN}")));
			return;
		}

		var plugin = Database.LoadPlugins().FirstOrDefault(x => x.Name == pN);
		Program.plugin = plugin;

		DiscordInteractionResponseBuilder modal = new();
		modal.WithTitle($"Editing {Program.PlugName}'s notes!").WithCustomId("edit-pluginnotes").AddComponents(
			new TextInputComponent("Notes:", "plugnotes", required: true, style: TextInputStyle.Paragraph, value: plugin.Description));

		await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
	}
}