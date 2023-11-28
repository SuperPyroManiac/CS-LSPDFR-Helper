using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class FindErrors : ApplicationCommandModule
{
    [SlashCommand("FindErrors", "Returns a list of all errors in the database that match the search parameters!")]
    [RequireTsRole]
    public static async Task FindErrorsCmd
    (
        InteractionContext ctx,
        [Option("ID", "The error id in the bot's database.")] string errId=null,
        [Option("Regex", "Regex for detecting the error.")] string regex=null,
        [Option("Solution", "Solution for the error.")] string solution=null,
        [Option("Description", "Description for the error.")] string description=null,
        [Option("Level", "Error level (WARN, SEVERE, CRITICAL).")] Level? level=null,
        [Option("Strict_Search", "true = enabled, false = disabled (approximate search)")] bool? exactMatch=false
    )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder { IsEphemeral = true }
        );
        
        try 
        {
            string searchParamsListForLog = FindErrorMessages.GetSearchParamsList(
                "Ran 'FindErrors' command with the following parameters:", 
                errId,
                regex,
                solution,
                description,
                level,
                exactMatch
            );
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info(searchParamsListForLog));

            List<Error> errorsFound = Database.FindErrors(errId, regex, solution, description, level, exactMatch);

            if (errorsFound.Count > 0) 
            {
                int resultsPerPage = 3;
                int currentResultsPerPage = 0;
                List<Page> pages = new List<Page>();
                string searchResultsHeader = FindErrorMessages.GetSearchParamsList(
                    $"I found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:",
                    errId,
                    regex,
                    solution,
                    description,
                    level,
                    exactMatch
                ) + "\r\nSearch results:";

                string currentPageContent = searchResultsHeader;
                for(int i=0; i < errorsFound.Count; i++)
                {
                    Error error = errorsFound[i];
                    currentPageContent += "\r\n\r\n"
                        + $"> **__Error ID {error.ID}__**\r\n"
                        + $"> **Regex:**\r\n> `{error.Regex.Replace("\n", "`\n> `")}`\r\n> \r\n" 
                        + $"> **Solution:**\r\n> {error.Solution.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **Description:**\r\n> {error.Description.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **Level:**\r\n> {error.Level}";
                    currentResultsPerPage++;
                    if (currentResultsPerPage == resultsPerPage || i == errorsFound.Count-1) {
                        var embed = BasicEmbeds.Generic(currentPageContent, DiscordColor.DarkBlue);
                        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Showing results {i+2 - currentResultsPerPage} - {i+1} (total: {errorsFound.Count})"
                        };
                        var page = new Page(embed: embed);
                        pages.Add(page);
                        currentPageContent = searchResultsHeader;
                        currentResultsPerPage = 0;
                    }
                }

                await ctx.Interaction.SendPaginatedResponseAsync(true, ctx.User, pages, asEditResponse: true);
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(
                        BasicEmbeds.Warning(
                            FindErrorMessages.GetSearchParamsList("No errors found with the following search parameters:", errId, regex, solution, description, level, exactMatch)
                        )
                    )
                );
            }
        } 
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
        }
    }
}