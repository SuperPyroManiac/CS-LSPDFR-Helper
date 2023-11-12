using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindPluginMessages : FindBaseMessages
{
    internal static string GetSearchParamsList(string title, string? plugName, string? plugDName, string? plugId, State? plugState, bool? exactMatch)
    {
        string searchParamsList = $"**{title}**\r\n";
        if (plugName != null)
            searchParamsList += $"- **Name:** *{plugName}*\r\n";
        if (plugDName != null)
            searchParamsList += $"- **Display Name:** *{plugDName}*\r\n";
        if (plugId != null)
            searchParamsList += $"- **ID (on lcpdfr.com):** *{plugId}*\r\n";
        if (plugState != null)
            searchParamsList += $"- **State:** *{plugState}*\r\n";
        if (exactMatch != null)
            searchParamsList += $"- **Strict search enabled:** *{exactMatch}*\r\n";;

        return searchParamsList;
    }

    internal static async Task SendDbOperationConfirmation(Plugin newPlugin, DbOperation operation, Plugin? oldPlugin=null, ModalSubmitEventArgs? e=null)
    {
        string pluginDbRowId = $"**DB ID:** {newPlugin.DbRowId}\r\n";
        string pluginDName = $"**Display Name:** {newPlugin.DName}\r\n";
        string pluginVersion = $"**Version:** {newPlugin.Version}\r\n";
        string pluginEaVersion = $"**Early Access Version:** {newPlugin.EAVersion}\r\n";
        string pluginId = $"**ID (on lcpdfr.com):** {newPlugin.ID}\r\n";
        string pluginLink = $"**Link:** {newPlugin.Link}\r\n";
        string pluginState = $"**State:** {newPlugin.State}";
        string pluginPropsList = pluginDbRowId + pluginDName + pluginVersion + pluginEaVersion + pluginId + pluginLink + pluginState;
        
        DiscordEmbedBuilder? embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"**Added {newPlugin.Name}!**\r\n" + pluginPropsList);
                break;
            
            case DbOperation.UPDATE:
                string title = $"**Modified {newPlugin.Name}!**\r\n";
                string text = title;

                List<ModifiedProperty> properties = new()
                {
                    new ModifiedProperty("DB ID", oldPlugin.DbRowId.ToString(), oldPlugin.DbRowId.ToString(), pluginDbRowId),
                    new ModifiedProperty("Display Name", oldPlugin.DName, newPlugin.DName, pluginDName),
                    new ModifiedProperty("Version", oldPlugin.Version, newPlugin.Version, pluginVersion),
                    new ModifiedProperty("Early Access Version", oldPlugin.EAVersion, newPlugin.EAVersion, pluginEaVersion),
                    new ModifiedProperty("ID (on lcpdfr.com)", oldPlugin.ID, newPlugin.ID, pluginId),
                    new ModifiedProperty("Link", oldPlugin.Link, newPlugin.Link, pluginLink),
                    new ModifiedProperty("State", oldPlugin.State, newPlugin.State, pluginState),
                };
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
        if (e != null && embed != null) 
        {
            await e.Interaction.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed)
            );
            Logging.sendLog(e.Interaction.Channel.Id, e.Interaction.User.Id, embed);
            return;
        }
        else 
            throw new NotImplementedException("This SendDbOperationConfirmation branch is not implemented yet.");
    }
} 