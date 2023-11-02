using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using ULSS_Helper.Modules.Messages;

namespace ULSS_Helper.Modules.Commands;

public class FindPlugins : ApplicationCommandModule
{
    [SlashCommand("FindPlugins", "Returns a list of all plugins in the database that match the search parameters!")]

    public static async Task FindPluginsCmd(InteractionContext ctx,
        [Option("Name", "The plugin's name.")] string? plugName=null,
        [Option("DName", "The plugin's display name.")] string? plugDName=null,
        [Option("ID", "The plugin's id on lcpdfr.com.")] string? plugId=null,
        [Option("State", "The plugin's state (LSPDFR, EXTERNAL, BROKEN, LIB).")] State? plugState=null,
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
        
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        
        try 
        {
            List<Plugin> pluginsFound = DatabaseManager.FindPlugins(plugName, plugDName, plugId, plugState, exactMatch);

            if (pluginsFound.Count > 0) 
            {
                int resultsPerPage = 3;
                int currentResultsPerPage = 0;
                int numberOfResults = pluginsFound.Count <= resultsPerPage ? pluginsFound.Count : resultsPerPage;
                List<Page> pages = new List<Page>();
                string searchResultsHeader = 
                    $"**I found {pluginsFound.Count} plugin{(pluginsFound.Count != 1 ? "s" : "")} that match{(pluginsFound.Count == 1 ? "es" : "")} the following search parameters:**\r\n"
                    + $"{(plugName != null ? "- Name: *"+plugName+"*\r\n" : "")}"
                    + $"{(plugDName != null ? "- Display Name: *"+plugDName+"*\r\n" : "")}"
                    + $"{(plugId != null ? "- ID (on lcpdfr.com): *"+plugId+"*\r\n" : "")}"
                    + $"{(plugState != null ? "- State: *"+plugState+"*\r\n" : "")}"
                    + $"{(exactMatch != null ? "- exactMatch: *"+exactMatch+"*\r\n" : "")}"
                    + "\n"
                    + $"Search results:";

                string currentPageContent = searchResultsHeader;
                for(int i=0; i < pluginsFound.Count; i++)
                {
                    Plugin plugin = pluginsFound[i];
                    currentPageContent += "\r\n\r\n"
                        + $"> **Plugin {plugin.Name}**\r"
                        + $"> Display Name: {plugin.DName}\r" 
                        + $"> Version: {plugin.Version}\r\n"
                        + $"> Early Access Version: {plugin.EAVersion}\r"
                        + $"> ID (on lcpdfr.com): {plugin.ID}\r"
                        + $"> Link: {plugin.Link}\r"
                        + $"> State: {plugin.State}";
                    currentResultsPerPage++;
                    if (currentResultsPerPage == resultsPerPage || i == pluginsFound.Count-1) {
                        var embed = BasicEmbeds.Generic(currentPageContent, DiscordColor.DarkBlue);
                        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Showing results {i+2 - currentResultsPerPage} - {i+1} (total: {pluginsFound.Count})"
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
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning(
                    $"**No plugins found with the following search parameters:**\r\n"
                    + $"{(plugName != null ? "- Name: *"+plugName+"*\r\n" : "")}"
                    + $"{(plugDName != null ? "- Display Name: *"+plugDName+"*\r\n" : "")}"
                    + $"{(plugId != null ? "- ID (on lcpdfr.com): *"+plugId+"*\r\n" : "")}"
                    + $"{(plugState != null ? "- State: *"+plugState+"*\r\n" : "")}"
                    + $"{(exactMatch != null ? "- exactMatch: *"+exactMatch+"*" : "")}"
                )));
                return;
            }
        }
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
            return;
        }
        
    }
}