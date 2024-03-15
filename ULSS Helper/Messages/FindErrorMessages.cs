using System.ComponentModel.Design;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Messages;

internal class FindErrorMessages : FindBaseMessages
{    
    internal static string GetSearchParamsList(string title, string errId, string regex, string solution, string description, Level? level, bool exactMatch)
    {
        var searchParamsList = $"**{title}**\r\n";
        if (errId != null)
            searchParamsList += $"- **ID:** {errId}\r\n";
        if (regex != null)
            searchParamsList += $"- **Regex:**\n```\n{regex}\n```\r\n";
        if (solution != null)
            searchParamsList += $"- **Solution:** ```{solution}```\r\n";
        if (description != null)
            searchParamsList += $"- **Description:** ```{description}```\r\n";
        if (level != null)
            searchParamsList += $"- **Level:** {level}\r\n";
        if (exactMatch)
            searchParamsList += $"- **Strict search:** enabled\r\n";

        return searchParamsList;
    }

    internal static async Task SendDbOperationConfirmation(Error newError, DbOperation operation, ulong channel, ulong sender, Error oldError = null)
    {
        var errorRegex = $"**Regex:**\r\n```\n{newError.Regex}\n```\r\n";
        var errorSolution = $"**Solution:**\n```{newError.Solution}```\r\n\r\n";
        var errorDescription = $"**Description:**\n```{newError.Description}```\r\n\r\n";
        var errorLevel = $"**Level:** {newError.Level}";
        var errorPropsList = errorRegex + errorSolution + errorDescription + errorLevel;

        DiscordEmbedBuilder embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"__Added a new error with ID {newError.ID}__\r\n>>> **Level:** {newError.Level}", true);
                break;
            
            case DbOperation.UPDATE:
                var title = $"__Modified error ID: {newError.ID}!__\r\n>>> ";
                var text = title;

                List<ModifiedProperty> properties =
                [
                    new ModifiedProperty("Regex", oldError!.Regex, newError.Regex, errorRegex),
                    new ModifiedProperty("Solution", oldError.Solution, newError.Solution, errorSolution),
                    new ModifiedProperty("Description", oldError.Description, newError.Description, errorDescription),
                    new ModifiedProperty("Level", oldError.Level, newError.Level, errorLevel)
                ];
                try 
                {
                    text += GetModifiedPropertiesList(properties); 
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + errorPropsList, true);
                    Console.WriteLine(value: exception);
                    break;
                }
                
                embed = BasicEmbeds.Info(text, true);
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