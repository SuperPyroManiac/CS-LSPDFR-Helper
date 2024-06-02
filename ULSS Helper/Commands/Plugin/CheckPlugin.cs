using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Plugin;

public class CheckPlugin : ApplicationCommandModule
{
	[SlashCommand("CheckPlugin", "Get info on a specific plugin.")]
	[RequireNotOnBotBlacklist]
	public async Task CheckPluginCmd(InteractionContext ctx,
		[Autocomplete(typeof(PluginAutoComplete)),
		Option("Plugin", "Plugin Name", true)] string plug)
	{
		var response = new DiscordInteractionResponseBuilder();
		response.IsEphemeral = true;
		var plugin = Program.Cache.GetPlugin(plug);
		
		if (plugin == null)
		{
			await ctx.CreateResponseAsync(response.AddEmbed(BasicEmbeds.Error($"No plugin found with name {plug}")));
			return;
		}
		
		if (!string.IsNullOrEmpty(plugin.Link)) plugin.Link = $"[Here]({plugin.Link})";
		plugin.Link ??= "N/A";
		
		var embed = new DiscordEmbedBuilder
		{
			Description =
				$"## __{plugin.Name}__\r\n"
				+ $">>> **Display Name:** {plugin.DName}\n"
				+ $"**Version:** {plugin.Version}\n"
				+ $"**Link:** {plugin.Link}\n"
				+ $"**State:** {plugin.State}\n"
				+ $"**Notes:** \r\n```{plugin.Description}```\r\n",
			Color = new DiscordColor(243, 154, 18),
			Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
			Footer = new DiscordEmbedBuilder.EmbedFooter
			{
				Text = "Provided by discord.gg/ulss"
			}
		};
		response.AddComponents([
			new DiscordButtonComponent(ButtonStyle.Secondary, "SendFeedback", "Send Feedback", false,
				new DiscordComponentEmoji("📨"))
		]);
		
		await ctx.CreateResponseAsync(response.AddEmbed(embed));
		await Logging.SendPubLog(BasicEmbeds.Info(
			$"__User checked a plugin!__\r\n"
			+ $">>> Sender: {ctx.Member.Mention} ({ctx.Member.Username})\r\n"
			+ $"Channel: <#{ctx.Channel.Id}>\r\n"
			+ $"Plugin: {plug}\r\n", true));
	}
}