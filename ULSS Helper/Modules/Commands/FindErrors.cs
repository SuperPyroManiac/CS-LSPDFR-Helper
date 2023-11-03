using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ULSS_Helper.Modules.Messages;

namespace ULSS_Helper.Modules.Commands;

public class FindErrors : ApplicationCommandModule
{
    [SlashCommand("FindErrors", "Returns a list of all errors in the database that match the search parameters!")]

    public static async Task FindErrorsCmd(InteractionContext ctx,
        [Option("ID", "The error id in the bot's database.")] string? errId=null,
        [Option("Regex", "Regex for detecting the error.")] string? regex=null,
        [Option("Solution", "Solution for the error.")] string? solution=null,
        [Option("Level", $"Error level (WARN, SEVERE, CRITICAL).")] Level? level=null,
        [Option("Strict_Search", "true = enabled, false = disabled (approximate search)")] bool? exactMatch=false
        )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder
            {
                IsEphemeral = true
            }
        );
        
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        try 
        {
            List<Error> errorsFound = DatabaseManager.FindErrors(errId, regex, solution, level, exactMatch);
            
            if (errorsFound.Count > 0) 
            {
                int resultsPerPage = 3;
                int currentResultsPerPage = 0;
                List<Page> pages = new List<Page>();
                string searchResultsHeader = GetSearchParamsList(
                    $"I found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:",
                    errId,
                    regex,
                    solution,
                    level,
                    exactMatch
                ) + "\nSearch results:";

                string currentPageContent = searchResultsHeader;
                for(int i=0; i < errorsFound.Count; i++)
                {
                    Error error = errorsFound[i];
                    currentPageContent += "\r\n\r\n"
                        + $"> **Error ID {error.ID}**\r"
                        + $"> Regex:\n> ```{error.Regex ?? " "}```\r" 
                        + $"> Solution:\n> ```{error.Solution ?? " "}```\r"
                        + $"> Level: {error.Level}";
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
                return;
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(
                        BasicEmbeds.Warning(
                            GetSearchParamsList("No errors found with the following search parameters:", errId, regex, solution, level, exactMatch)
                        )
                    )
                );
                return;
            }
        } 
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
            return;
        }
    }

    private static string GetSearchParamsList(string title, string? errId, string? regex, string? solution, Level? level, bool? exactMatch) 
    {
        return $"**{title}**\r\n"
            + $"{(errId != null ? "- ID: *"+errId+"*\n" : "")}"
            + $"{(regex != null ? "- Regex:\n```"+regex+"```\n" : "")}"
            + $"{(solution != null ? "- Solution:\n```"+solution+"```\n" : "")}"
            + $"{(level != null ? "- Level: *"+level+"*\n" : "")}"
            + $"{(exactMatch != null ? "- Strict search enabled: *"+exactMatch+"*\n" : "")}";
    }
}