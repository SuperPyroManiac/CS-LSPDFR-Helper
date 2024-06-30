using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.CustomTypes.SpecialTypes;

namespace LSPDFR_Helper.Functions.Messages.ModifiedProperties;

public abstract class FindErrorMessages : FindBaseMessages
{    
    public static string GetSearchParamsList(string title, string errId, string pattern, string solution, string description, Level? level, bool exactMatch)
    {
        var searchParamsList = $"**{title}**\r\n";
        if (errId != null)
            searchParamsList += $"- **Id:** {errId}\r\n";
        if (pattern != null)
            searchParamsList += $"- **Pattern:**\n```\n{pattern}\n```\r\n";
        if (solution != null)
            searchParamsList += $"- **Solution:** ```{solution}```\r\n";
        if (description != null)
            searchParamsList += $"- **Description:** ```{description}```\r\n";
        if (level != null)
            searchParamsList += $"- **Level:** {level}\r\n";
        if (exactMatch)
            searchParamsList += "- **Strict search:** enabled\r\n";

        return searchParamsList;
    }

    public static async Task SendDbOperationConfirmation(Error newError, DbOperation operation, ulong channel, ulong sender, Error oldError = null)
    {
        var errorRegex = $"**Regex:**\r\n```\n{newError.Pattern}\n```\r\n";
        var errorSolution = $"**Solution:**\n```{newError.Solution}```\r\n\r\n";
        var errorDescription = $"**Description:**\n```{newError.Description}```\r\n\r\n";
        var errorStringmatch = $"**String Match:**\n`{newError.StringMatch}`\r\n";
        var errorLevel = $"**Level:** {newError.Level}";
        var errorPropsList = errorRegex + errorSolution + errorDescription + errorStringmatch + errorLevel;

        DiscordEmbedBuilder embed = null;
        switch (operation)
        {
            case DbOperation.CREATE:
                embed = BasicEmbeds.Info($"__Added a new error with ID {newError.Id}__\r\n>>> **String Match:** `{newError.StringMatch}`\r\n**Level:** `{newError.Level}`");
                break;
            
            case DbOperation.UPDATE:
                var title = $"__Modified error ID: {newError.Id}!__\r\n>>> ";
                var text = title;

                List<ModifiedProperty> properties =
                [
                    new ModifiedProperty("Regex", oldError!.Pattern, newError.Pattern, errorRegex),
                    new ModifiedProperty("Solution", oldError.Solution, newError.Solution, errorSolution),
                    new ModifiedProperty("Description", oldError.Description, newError.Description, errorDescription),
                    new ModifiedProperty("String Match", oldError.StringMatch.ToString(), newError.StringMatch.ToString(), errorStringmatch),
                    new ModifiedProperty("Level", oldError.Level.ToString(), newError.Level.ToString(), errorLevel)
                ];
                try 
                {
                    text += GetModifiedPropertiesList(properties); 
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + errorPropsList);
                    Console.WriteLine(exception);
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