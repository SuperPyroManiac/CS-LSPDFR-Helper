using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;

namespace ULSS_Helper.Commands.Error;

public class RemoveError
{
    [Command("RemoveError")]
    [Description("Removes an error from the database!")]
    public async Task RemoveErrorCmd
    (SlashCommandContext ctx, [Description("Must match an existing error id!")] string errorId)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var error in Database.LoadErrors())
        {
            if (error.ID == errorId)
            {
                isValid = true;
                await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource, 
                    bd.AddEmbed(BasicEmbeds.Success($"**Removed error with id: {errorId}**")));
                await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning(
                    $"Removed error: {errorId}!\r\n>>> " +
                    $"**Regex:**\r\n" +
                    $"```{error.Regex}```\r\n" +
                    $"**Solution:**\r\n" +
                    $"```{error.Solution}```\r\n" +
                    $"**Description:**\r\n" +
                    $"```{error.Description}```\r\n" +
                    $"**Error Level: {error.Level}**", true));
                Database.DeleteError(error);
                return;
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!isValid)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Error($"**No error found with id: {errorId}!**")));
        }
    }
}