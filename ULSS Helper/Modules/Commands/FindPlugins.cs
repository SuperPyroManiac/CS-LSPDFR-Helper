using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class FindPlugins : ApplicationCommandModule
{
    [SlashCommand("FindPlugins", "Returns a list of all plugins in the database that match the search parameters!")]
    public async Task FindPluginsCmd(InteractionContext ctx,
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
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }
        
        try 
        {
            List<Plugin> pluginsFound = DatabaseManager.FindPlugins(plugName, plugDName, plugId, plugState, exactMatch);

            if (pluginsFound.Count > 0) 
            {
                int limit = 3;
                int numberOfResults = pluginsFound.Count <= limit ? pluginsFound.Count : limit;
                var response = new DiscordWebhookBuilder();
                response.AddEmbed(MessageManager.Generic(
                    $"**I found {pluginsFound.Count} plugin{(pluginsFound.Count != 1 ? "s" : "")} that match{(pluginsFound.Count == 1 ? "es" : "")} the following search parameters:**\r\n"
                    + $"{(plugName != null ? "- Name: *"+plugName+"*\r\n" : "")}"
                    + $"{(plugDName != null ? "- Display Name: *"+plugDName+"*\r\n" : "")}"
                    + $"{(plugId != null ? "- ID (on lcpdfr.com): *"+plugId+"*\r\n" : "")}"
                    + $"{(plugState != null ? "- State: *"+plugState+"*\r\n" : "")}"
                    + $"{(exactMatch != null ? "- exactMatch: *"+exactMatch+"*\r\n" : "")}"
                    + "\n"
                    + $"Showing {numberOfResults} of {pluginsFound.Count} results:", DiscordColor.DarkGreen
                ));
                for(int i=0; i < numberOfResults; i++)
                {
                    Plugin plugin = pluginsFound[i];
                    response.AddEmbed(MessageManager.Generic(
                        $"**Plugin {plugin.Name}**\r\n"
                        + $"Display Name: {plugin.DName}\r\n" 
                        + $"Version: {plugin.Version}\r\n"
                        + $"Early Access Version: {plugin.EAVersion}\r\n"
                        + $"ID (on lcpdfr.com): {plugin.ID}\r\n"
                        + $"Link: {plugin.Link}\r\n"
                        + $"State: {plugin.State}",
                        DiscordColor.DarkBlue
                    ));
                }
                await ctx.EditResponseAsync(response);
                return;
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Warning(
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
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Error(e.Message)));
            return;
        }
        
    }
}