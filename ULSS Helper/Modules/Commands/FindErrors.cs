using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class FindErrors : ApplicationCommandModule
{
    [SlashCommand("FindErrors", "Returns a list of all errors in the database that match the search parameters!")]
    public async Task FindErrorsCmd(InteractionContext ctx,
        [Option("ID", "The error id in the bot's database.")] string? errId=null,
        [Option("Regex", "Regex for detecting the error.")] string? regex=null,
        [Option("Solution", "Solution for the error.")] string? solution=null,
        [Option("Level", $"Error level (WARN, SEVERE).")] Level? level=null,
        [Option("exactMatch", "Exact = true, approximate = false")] bool? exactMatch=false
        )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true
            }
        );
        
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }
        try 
        {
            List<Error> errorsFound = DatabaseManager.FindErrors(errId, regex, solution, level, exactMatch);
            if (errorsFound.Count > 0) 
            {
                int limit = 3;
                int numberOfResults = errorsFound.Count <= limit ? errorsFound.Count : limit;
                var response = new DiscordWebhookBuilder();
                response.AddEmbed(MessageManager.Generic(
                    $"**I found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:**\r\n"
                    + $"{(errId != null ? "- ID: *"+errId+"*\r\n" : "")}"
                    + $"{(regex != null ? "- Regex:\n```"+regex+"```\r\n" : "")}"
                    + $"{(solution != null ? "- Solution:\n```"+solution+"```\r\n" : "")}"
                    + $"{(level != null ? "- Level: *"+level+"*\r\n" : "")}"
                    + $"{(exactMatch != null ? "- exactMatch: *"+exactMatch+"*\r\n" : "")}"
                    + "\n"
                    + $"Showing {numberOfResults} of {errorsFound.Count} results:", DiscordColor.DarkGreen
                ));
                for(int i=0; i < numberOfResults; i++)
                {
                    Error error = errorsFound[i];
                    response.AddEmbed(MessageManager.Generic(
                        $"**Error ID {error.ID}**\r\n"
                        + $"Regex:\n```{error.Regex ?? " "}```\r\n" 
                        + $"Solution:\n```{error.Solution ?? " "}```\r\n"
                        + $"Level: {error.Level}",
                        DiscordColor.DarkBlue
                    ));
                }
                await ctx.EditResponseAsync(response);
                return;
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Warning(
                    $"**No errors found with the following search parameters:**\r\n"
                    + $"{(errId != null ? "- ID: *"+errId+"*\r\n" : "")}"
                    + $"{(regex != null ? "- Regex:\n```"+regex+"```\r\n" : "")}"
                    + $"{(solution != null ? "- Solution:\n```"+solution+"```\r\n" : "")}"
                    + $"{(level != null ? "- Level: *"+level+"*\r\n" : "")}"
                    + $"{(exactMatch != null ? "- exactMatch: *"+exactMatch+"*\r\n" : "")}"
                )));
                return;
            }
        } 
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Error(e.Message)));
            return;
        }
    }
}