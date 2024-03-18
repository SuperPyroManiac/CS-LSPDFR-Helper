using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class RemoveError : ApplicationCommandModule
{
    [SlashCommand("RemoveError", "Removes an error from the database!")]
    [RequireAdvancedTsRole]
    public async Task RemoveErrorCmd
    (
        InteractionContext ctx,
        [Option("ID", "Must match an existing error id!")] string errorId
    )
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var error in Database.LoadErrors())
        {
            if (error.ID == errorId)
            {
                isValid = true;
                await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"**Removed error with id: {errorId}**")));
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
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"**No error found with id: {errorId}!**")));
        }
    }
}