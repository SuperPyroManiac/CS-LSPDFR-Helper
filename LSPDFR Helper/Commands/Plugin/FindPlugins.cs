using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFR_Helper.CustomTypes.AutoCompleteTypes;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Messages.ModifiedProperties;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Plugin;

public class FindPlugins
{
    [Command("findplugins")]
    [Description("Returns a list of all plugins in the database that match the search parameters!")]
    public static async Task FindPluginsCmd(SlashCommandContext ctx,
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string plugname=null,
        [Description("The plugin's display name.")] string plugdname=null,
        [Description("The plugin's id on lcpdfr.com.")] string plugid=null,
        [Description("The plugin's state.")] State? plugstate = null,
        [Description("The plugin's type.")] PluginType? plugtype = null,
        [Description("The plugin's description.")] string plugdescription=null,
        [Description("true = enabled, false = disabled (approximate search)")] bool exactmatch=false
        )
    {
        if (!await PermissionManager.RequireTs(ctx)) return;
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder { IsEphemeral = true });
        
        try 
        {
            var searchParamsListForLog = FindPluginMessages.GetSearchParamsList(
                "Ran 'FindPlugins' command with the following parameters:", 
                plugname, 
                plugdname, 
                plugid, 
                plugstate, 
                plugtype,
                plugdescription,
                exactmatch
            );
            await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info(searchParamsListForLog));
            
            var pluginsFound = DbManager.FindPlugins(plugname, plugdname, plugid, plugstate, plugtype, plugdescription, exactmatch);

            if (pluginsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindPluginMessages.GetSearchParamsList(
                    $"## __Found {pluginsFound.Count} plugin{(pluginsFound.Count != 1 ? "s" : "")} that match{(pluginsFound.Count == 1 ? "es" : "")} the following search parameters:__", 
                    plugname, 
                    plugdname, 
                    plugid, 
                    plugstate, 
                    plugtype,
                    plugdescription,
                    exactmatch
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
                        + $"> **Early Access Version:** {plugin.EaVersion}\r\n"
                        + $"> **Id (on lcpdfr.com):** {plugin.Id}\r\n"
                        + $"> **Link:** {plugin.Link}\r\n"
                        + $"> **Type:** {plugin.PluginType}\r\n"
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
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning(FindPluginMessages.GetSearchParamsList("No plugins found with the following search parameters:", plugname, plugdname, plugid, plugstate, plugtype, plugdescription, exactmatch))));
            }
        }
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
        }
    }
}