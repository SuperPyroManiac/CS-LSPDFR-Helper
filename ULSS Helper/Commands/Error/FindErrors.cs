using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Error;

public class FindErrors
{
    [Command("FindErrors")]
    [Description("Returns a list of all errors in the database that match the search parameters!")]
    public static async Task FindErrorsCmd
    (SlashCommandContext ctx,
        [Description("The error id in the bot's database.")] string errId=null,
        [Description("Regex for detecting the error.")] string regex=null,
        [Description("Solution for the error.")] string solution=null,
        [Description("Description for the error.")] string description=null,
        [Description("Error level (WARN, SEVERE, CRITICAL).")] Level? level=null,
        [Description("true = enabled, false = disabled (approximate search)")] bool exactMatch=false)
    {
        if (!await PermissionManager.RequireTs(ctx)) return;
        await ctx.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder { IsEphemeral = true }
        );
        
        try 
        {
            var searchParamsListForLog = FindErrorMessages.GetSearchParamsList(
                "Ran 'FindErrors' command with the following parameters:", 
                errId,
                regex,
                solution,
                description,
                level,
                exactMatch
            );
            await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info(searchParamsListForLog));

            var errorsFound = Database.FindErrors(errId, regex, solution, description, level, exactMatch);

            if (errorsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindErrorMessages.GetSearchParamsList(
                    $"I found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:",
                    errId,
                    regex,
                    solution,
                    description,
                    level,
                    exactMatch
                ) + "\r\nSearch results:";

                var currentPageContent = searchResultsHeader;
                for(var i=0; i < errorsFound.Count; i++)
                {
                    var error = errorsFound[i];
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