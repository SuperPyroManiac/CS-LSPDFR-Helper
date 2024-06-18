using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Commands.Error;

public class RemoveError
{
    [Command("removeerror")]
    [Description("Removes an error from the database!")]
    public async Task RemoveErrorCmd
    (SlashCommandContext ctx, [Description("Must match an existing error id!")] string errorId)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var error = DbManager.GetError(errorId);
        if (error != null)
        {
            await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Success($"**Removed error with id: {errorId}**")));
            await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning(
                $"Removed error: {errorId}!\r\n>>> " +
                $"**Regex:**\r\n" +
                $"```{error.Pattern}```\r\n" +
                $"**Solution:**\r\n" +
                $"```{error.Solution}```\r\n" +
                $"**Description:**\r\n" +
                $"```{error.Description}```\r\n" +
                $"**String Match:**\r\n" +
                $"```{error.StringMatch}```\r\n" +
                $"**Error Level: {error.Level}**", true));
            DbManager.DeleteError(error);
            return;
        }
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
            bd.AddEmbed(BasicEmbeds.Error($"**No error found with id: {errorId}!**")));
    }
}