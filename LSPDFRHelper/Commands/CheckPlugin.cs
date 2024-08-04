using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands;

public class CheckPlugin
{
	[Command("checkplugin")]
	[Description("Get info on a specific plugin.")]
	public async Task CheckPluginCmd(SlashCommandContext ctx, 
		[Description("Must match an existing plugin name!"), 
		 SlashAutoCompleteProvider<PluginAutoComplete>] string name)
	{
		if (!await PermissionManager.RequireNotBlacklisted(ctx)) return;
		var response = new DiscordInteractionResponseBuilder();
		response.IsEphemeral = true;
		var plugin = Program.Cache.GetPlugin(name);
		
		if (plugin == null)
		{
			await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
				response.AddEmbed(BasicEmbeds.Error($"No plugin found with name {name}")));
			return;
		}

        var link = "";
		if (!string.IsNullOrEmpty(plugin.Link)) link = $"[Here]({plugin.Link})";
		link ??= "N/A";
		
		var embed = BasicEmbeds.Public(            
            $"## __{plugin.Name}__{BasicEmbeds.AddBlanks(25)}\r\n"
            + $">>> **Display Name:** {plugin.DName}\n"
            + $"**Version:** {plugin.Version}\n"
            + $"**Link:** {link}\n"
            + $"**Type:** {plugin.PluginType} | **State:** {plugin.State}\n"
            + $"**Notes:** \r\n```{plugin.Description}```\r\n");
		
		await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response.AddEmbed(embed));
		// await Logging.SendPubLog(BasicEmbeds.Info(
		// 	$"__User checked a plugin!__{BasicEmbeds.AddBlanks(25)}\r\n"
		// 	+ $">>> Sender: {ctx.Member!.Mention} ({ctx.Member.Username})\r\n"
		// 	+ $"Channel: <#{ctx.Channel.Id}>\r\n"
		// 	+ $"Plugin: {name}\r\n"));
	}
}