using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindPluginMessages : FindBaseMessages
{
    internal static string GetSearchParamsList(string title, string plugName, string plugDName, string plugId, State? plugState, string plugDescription, bool exactMatch)
    {
        var searchParamsList = $"**{title}**\r\n";
        if (plugName != null)
            searchParamsList += $"- **Name:** {plugName}\r\n";
        if (plugDName != null)
            searchParamsList += $"- **Display Name:** {plugDName}\r\n";
        if (plugId != null)
            searchParamsList += $"- **ID (on lcpdfr.com):** {plugId}\r\n";
        if (plugState != null)
            searchParamsList += $"- **State:**\r\n {plugState}\r\n";
        if (plugDescription != null)
            searchParamsList += $"- **Notes:** \r\n> {plugDescription.Replace("\n", "\n> ")}\r\n";
        if (exactMatch)
            searchParamsList += $"- **Strict search:** enabled\r\n";

        return searchParamsList;
    }

    internal static async Task SendDbOperationConfirmation(Plugin newPlugin, DbOperation operation, ulong channel, ulong sender, Plugin oldPlugin = null)
    {
        var pluginDName = $"**Display Name:** {newPlugin.DName}\r\n";
        var pluginVersion = $"**Version:** {newPlugin.Version}\r\n";
        var pluginEaVersion = $"**EA Version:** {newPlugin.EAVersion}\r\n";
        var pluginId = $"**ID:** {newPlugin.ID}\r\n";
        var pluginLink = $"**Link:** {newPlugin.Link}\r\n";
        var pluginDescription = $"**Notes:** \r\n> {newPlugin.Description.Replace("\n", "\n> ")}\r\n";
        var pluginState = $"**State:** {newPlugin.State}\r\n";
        var pluginPropsList = pluginDName + pluginVersion + pluginEaVersion + pluginId + pluginDescription + pluginLink + pluginState;
        
        DiscordEmbedBuilder embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"**Added new plugin: {newPlugin.Name}**\r\n>>> **State:** {newPlugin.State}", true);
                break;
            
            case DbOperation.UPDATE:
                var title = $"__Modified {newPlugin.Name}!__\r\n>>> ";
                var text = title;

                List<ModifiedProperty> properties =
                [
                    new ModifiedProperty("Display Name", oldPlugin.DName, newPlugin.DName, pluginDName),
                    new ModifiedProperty("Version", oldPlugin.Version, newPlugin.Version, pluginVersion),
                    new ModifiedProperty("EA Version", oldPlugin.EAVersion, newPlugin.EAVersion, pluginEaVersion),
                    new ModifiedProperty("ID", oldPlugin.ID, newPlugin.ID, pluginId),
                    new ModifiedProperty("Link", oldPlugin.Link, newPlugin.Link, pluginLink),
                    new ModifiedProperty("Notes", oldPlugin.Description, newPlugin.Description, pluginDescription),
                    new ModifiedProperty("State", oldPlugin.State, newPlugin.State, pluginState)
                ];
                try 
                {
                    text += GetModifiedPropertiesList(properties);
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + pluginPropsList, true);
                    Console.WriteLine(value: exception);
                    break;
                }
                embed = BasicEmbeds.Info(text);
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"{ChangesCount} {(ChangesCount == 1 ? "property has" : "properties have")} been modified."
                };
                ChangesCount = 0;
                break;
        }
        if (embed != null) 
        {
            var bd = new DiscordInteractionResponseBuilder();
            bd.IsEphemeral = true;
            Logging.SendLog(channel, sender, embed);
        }
        else 
            throw new NotImplementedException("This SendDbOperationConfirmation branch is not implemented yet.");
    }
} 