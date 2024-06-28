using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Messages.ModifiedProperties;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Error;

public class FindErrors
{
    [Command("finderrors")]
    [Description("Returns a list of all errors in the database that match the search parameters!")]
    public static async Task FindErrorsCmd
    (SlashCommandContext ctx,
        [Description("The error id in the bot's database.")] string id = null,
        [Description("Pattern for detecting the error.")] string pattern = null,
        [Description("Solution for the error.")] string solution = null,
        [Description("Description for the error.")] string description = null,
        [Description("Error level")] Level? level = null,
        [Description("true = enabled, false = disabled (approximate search)")] bool exactmatch = false)
    {
        if (!await PermissionManager.RequireTs(ctx)) return;
        await ctx.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder { IsEphemeral = true });
        
        try 
        {
            var errorsFound = DbManager.FindErrors(id, pattern, solution, description, level, exactmatch);

            if (errorsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindErrorMessages.GetSearchParamsList($"### __Found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:__",
                    id,
                    pattern,
                    solution,
                    description,
                    level,
                    exactmatch
                ) + "\r\nSearch results:";

                var currentPageContent = searchResultsHeader;
                for(var i=0; i < errorsFound.Count; i++)
                {
                    var error = errorsFound[i];
                    currentPageContent += "\r\n\r\n"
                        + $"> ### __Error Id {error.Id}__\r\n"
                        + $"> **Pattern:**\r\n> `{error.Pattern.Replace("\n", "`\n> `")}`\r\n> \r\n" 
                        + $"> **Solution:**\r\n> {error.Solution.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **Description:**\r\n> {error.Description.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **String Match:** `{error.StringMatch}`\r\n"
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
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning(
                    FindErrorMessages.GetSearchParamsList("No errors found with the following search parameters:", id, pattern, solution, description, level, exactmatch))));
            }
        } 
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
        }
    }
}