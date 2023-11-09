using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class ErrorCmdMessages : DbCmdMessages
{    
    internal static string GetSearchParamsList(string title, string? errId, string? regex, string? solution, Level? level, bool? exactMatch) 
    {
        string searchParamsList = $"**{title}**\r\n";
        if (errId != null)
            searchParamsList += $"- **ID:** *{errId}*\r\n";
        if (regex != null)
            searchParamsList += $"- **Regex:**\n```\n{regex}\n```\r\n";
        if (solution != null)
            searchParamsList += $"- **Solution:**\n```\n{solution}\n```\r\n";
        if (level != null)
            searchParamsList += $"- **Level:** *{level}*\r\n";
        if (exactMatch != null)
            searchParamsList += $"- **Strict search enabled:** *{exactMatch}*\r\n";

        return searchParamsList;
    }

    internal static async Task SendDbOperationConfirmation(Error newError, DbOperation operation, Error? oldError=null, ModalSubmitEventArgs? e=null)
    {
        string errorRegex = $"**Regex:**\r\n```\n{newError.Regex}\n```\r\n";
        string errorSolution = $"**Solution:**\r\n```\n{newError.Solution}\n```\r\n";
        string errorLevel = $"**Level:** {newError.Level}";
        string errorPropsList = errorRegex + errorSolution + errorLevel;

        DiscordEmbedBuilder? embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"**Added a new error with ID {newError.ID}**\r\n" + errorPropsList);
                break;
            
            case DbOperation.UPDATE:
                string title = $"**Modified error ID: {newError.ID}!**\r\n";
                string text = title;

                List<ModifiedProperty> properties = new()
                {
                    new ModifiedProperty("Regex", oldError.Regex, newError.Regex, errorRegex),
                    new ModifiedProperty("Solution", oldError.Solution, newError.Solution, errorSolution),
                    new ModifiedProperty("Level", oldError.Level, newError.Level, errorLevel),
                };
                try 
                {
                    text += GetModifiedPropertiesList(properties); 
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + errorPropsList);
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