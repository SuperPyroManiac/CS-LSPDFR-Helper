using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.CustomTypes.SpecialTypes;

namespace LSPDFRHelper.Functions.Messages.ModifiedProperties;

public abstract class FindPluginMessages : FindBaseMessages
{
    public static string GetSearchParamsList(string title, string plugName, string plugDName, string plugId, State? plugState, PluginType? plugType, string plugDescription, bool exactMatch)
    {
        var searchParamsList = $"**{title}**\r\n";
        if (plugName != null)
            searchParamsList += $"- **Name:** {plugName}\r\n";
        if (plugDName != null)
            searchParamsList += $"- **Display Name:** {plugDName}\r\n";
        if (plugId != null)
            searchParamsList += $"- **Id (on lcpdfr.com):** {plugId}\r\n";
        if (plugState != null)
            searchParamsList += $"- **State:**\r\n {plugState}\r\n";
        if (plugType != null)
            searchParamsList += $"- **Type:**\r\n {plugType}\r\n";
        if (plugDescription != null)
            searchParamsList += $"- **Notes:** \r\n> {plugDescription.Replace("\n", "\n> ")}\r\n";
        if (exactMatch)
            searchParamsList += "- **Strict search:** enabled\r\n";

        return searchParamsList;
    }

    public static async Task SendDbOperationConfirmation(Plugin newPlugin, DbOperation operation, ulong channel, ulong sender, Plugin oldPlugin = null)
    {
        var pluginDName = $"**Display Name:** {newPlugin.DName}\r\n";
        var pluginVersion = $"**Version:** {newPlugin.Version}\r\n";
        var pluginEaVersion = $"**Ea Version:** {newPlugin.EaVersion}\r\n";
        var pluginId = $"**Id:** {newPlugin.Id}\r\n";
        var pluginLink = $"**Link:** {newPlugin.Link}\r\n";
        var plugAnnounce = $"**Announce:** {newPlugin.Announce}";
        var pluginDescription = $"**Notes:** \r\n```{newPlugin.Description}```\r\n";
        var pluginType = $"**Type:** {newPlugin.PluginType}\r\n";
        var pluginState = $"**State:** {newPlugin.State}\r\n";
        var pluginPropsList = pluginDName + pluginVersion + pluginEaVersion + pluginId + plugAnnounce + pluginDescription + pluginLink + pluginType + pluginState;
        
        DiscordEmbedBuilder embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"__Added new plugin: {newPlugin.Name}__\r\n>>> **Type:** {newPlugin.PluginType}\r\n**State:** {newPlugin.State}");
                break;
            
            case DbOperation.UPDATE:
                var title = $"__Modified {newPlugin.Name}!__\r\n>>> ";
                var text = title;

                List<ModifiedProperty> properties =
                [
                    new ModifiedProperty("Display Name", oldPlugin.DName, newPlugin.DName, pluginDName),
                    new ModifiedProperty("Version", oldPlugin.Version, newPlugin.Version, pluginVersion),
                    new ModifiedProperty("Ea Version", oldPlugin.EaVersion, newPlugin.EaVersion, pluginEaVersion),
                    new ModifiedProperty("Id", oldPlugin.Id.ToString(), newPlugin.Id.ToString(), pluginId),
                    new ModifiedProperty("Link", oldPlugin.Link, newPlugin.Link, pluginLink),
                    new ModifiedProperty("Announce", oldPlugin.Announce.ToString(), newPlugin.Announce.ToString(), plugAnnounce),
                    new ModifiedProperty("Notes", oldPlugin.Description, newPlugin.Description, pluginDescription),
                    new ModifiedProperty("Type", oldPlugin.PluginType.ToString(), newPlugin.PluginType.ToString(), pluginType),
                    new ModifiedProperty("State", oldPlugin.State.ToString(), newPlugin.State.ToString(), pluginState)
                ];
                try 
                {
                    text += GetModifiedPropertiesList(properties);
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + pluginPropsList);
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

        if ( operation == DbOperation.UPDATE && embed.Footer.Text.Contains('0') ) return;
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        await Logging.SendLog(channel, sender, embed);
    }
} 