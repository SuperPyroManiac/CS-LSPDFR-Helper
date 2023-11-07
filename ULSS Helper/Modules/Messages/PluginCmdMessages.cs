using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Modules.Messages;

internal class PluginCmdMessages : DbCmdMessages
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
                
                List<string> labels = new List<string>(
                    new string[] 
                    { 
                        "Name", // should be skipped
                        "DB ID",
                        "Display Name",
                        "Version",
                        "Early Access Version",
                        "ID (on lcpdfr.com)",
                        "Link",
                        "State"
                    }
                );
                List<string> defaultPropLines = new List<string>(
                    new string[] 
                    { 
                        SHOULD_BE_SKIPPED, // Plugin.Name
                        pluginDbRowId,
                        pluginDName,
                        pluginVersion,
                        pluginEaVersion,
                        pluginId,
                        pluginLink,
                        pluginState
                    }
                );
                try 
                {
                text += GetModifiedPropertiesList(oldPlugin, newPlugin, labels, defaultPropLines);
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + pluginPropsList);
                    Console.WriteLine(value: exception);
                    break;
                }
                /* for (int i=0; i < newPlugin.GetType().GetProperties().Length; i++)
                {
                    PropertyInfo oldPluginProp =  oldPlugin.GetType().GetProperties()[i];
                    PropertyInfo newPluginProp =  newPlugin.GetType().GetProperties()[i];
                    Type? type = Nullable.GetUnderlyingType(newPluginProp.PropertyType) ?? newPluginProp.PropertyType;

                    string? oldPluginPropValue = oldPluginProp.GetValue(oldPlugin)?.ToString();
                    string? newPluginPropValue = newPluginProp.GetValue(newPlugin)?.ToString();
                    if (oldPluginPropValue == null || newPluginPropValue == null || defaultPropLines[i].Equals(SHOULD_BE_SKIPPED))
                        continue;
                    
                    if (oldPluginPropValue.Equals(newPluginPropValue))
                    {
                        text += defaultPropLines[i];
                    }
                    else 
                    {
                        string label = labels[i];
                        text += $"**{label}:**\r\n```diff\r\n";
                        text += $"- {oldPluginPropValue}\r\n";
                        text += $"+ {newPluginPropValue}\r\n";
                        text += "```\r\n";
                        countChanges++;
                    }
                }
                 */
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