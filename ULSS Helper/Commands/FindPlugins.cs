using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class FindPlugins : ApplicationCommandModule
{
    [SlashCommand("FindPlugins", "Returns a list of all plugins in the database that match the search parameters!")]
    [RequireTsRoleSlash]
    public static async Task FindPluginsCmd(InteractionContext ctx,
        [Autocomplete(typeof(PluginAutoComplete)),Option("Name", "The plugin's name.")] string plugName=null,
        [Option("DName", "The plugin's display name.")] string plugDName=null,
        [Option("ID", "The plugin's id on lcpdfr.com.")] string plugId=null,
        [Option("State", "The plugin's state (LSPDFR, EXTERNAL, BROKEN, LIB).")] State? plugState=null,
        [Option("Description", "The plugin's description.")] string plugDescription=null,
        [Option("Strict_Search", "true = enabled, false = disabled (approximate search)")] bool exactMatch=false
        )
    {
        await ctx.CreateResponseAsync(
            InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder { IsEphemeral = true }
        );
        
        try 
        {
            var searchParamsListForLog = FindPluginMessages.GetSearchParamsList(
                "Ran 'FindPlugins' command with the following parameters:", 
                plugName, 
                plugDName, 
                plugId, 
                plugState, 
                plugDescription,
                exactMatch
            );
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info(searchParamsListForLog));
            
            var pluginsFound = Database.FindPlugins(plugName, plugDName, plugId, plugState, plugDescription, exactMatch);

            if (pluginsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindPluginMessages.GetSearchParamsList(
                    $"I found {pluginsFound.Count} plugin{(pluginsFound.Count != 1 ? "s" : "")} that match{(pluginsFound.Count == 1 ? "es" : "")} the following search parameters:", 
                    plugName, 
                    plugDName, 
                    plugId, 
                    plugState, 
                    plugDescription,
                    exactMatch
                ) + "\r\nSearch results:";

                var currentPageContent = searchResultsHeader;
                for(var i=0; i < pluginsFound.Count; i++)
                {
                    var plugin = pluginsFound[i];
                    var plugDesc = "N/A";
                    if (!string.IsNullOrEmpty(plugin.Description)) plugDesc = plugin.Description;
                    currentPageContent += "\r\n\r\n"
                        + $"> ### __Plugin: {plugin.Name}__\r\n"
                        + $"> **Display Name:** {plugin.DName}\r\n" 
                        + $"> **Version:** {plugin.Version}\r\n"
                        + $"> **Early Access Version:** {plugin.EAVersion}\r\n"
                        + $"> **ID (on lcpdfr.com):** {plugin.ID}\r\n"
                        + $"> **Link:** {plugin.Link}\r\n"
                        + $"> **State:** {plugin.State}\r\n"
                        + $"> **Notes:** \r\n> {plugDesc.Replace("\n", "\n> ")}";
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
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .AddEmbed(
                        BasicEmbeds.Warning(
                            FindPluginMessages.GetSearchParamsList("No plugins found with the following search parameters:", plugName, plugDName, plugId, plugState, plugDescription, exactMatch)
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