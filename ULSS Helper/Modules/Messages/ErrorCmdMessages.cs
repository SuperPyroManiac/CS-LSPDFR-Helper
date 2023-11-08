using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules.Messages;

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

                List<string> labels = new List<string>(
                    new string[] 
                    { 
                        "ID", // should be skipped
                        "Regex",
                        "Solution",
                        "Level" 
                    }
                );
                List<string> defaultPropLines = new List<string>(
                    new string[] 
                    { 
                        SHOULD_BE_SKIPPED, // Error.ID
                        errorRegex,
                        errorSolution,
                        errorLevel 
                    }
                );
                try 
                {
                    text += GetModifiedPropertiesList(oldError, newError, labels, defaultPropLines); 
                }
                catch (Exception exception)
                {
                    embed = BasicEmbeds.Info(title + errorPropsList);
                    Console.WriteLine(value: exception);
                    break;
                }
                
                /* 
                if (oldError.Regex.Equals(newError.Regex))
                {
                    text += errorRegex;
                }
                else 
                {
                    text += $"**Regex:**\r\n```diff\r\n";
                    text += $"- {oldError.Regex}\r\n";
                    text += $"+ {newError.Regex}\r\n";
                    text += "```\r\n";
                    countChanges++;
                }

                if (oldError.Solution.Equals(newError.Solution))
                {
                    text += errorSolution;
                }
                else 
                {
                    text += $"**Solution:**\r\n```diff\r\n";
                    text += $"- {oldError.Solution}\r\n";
                    text += $"+ {newError.Solution}\r\n";
                    text += "```\r\n";
                    countChanges++;
                }

                if (oldError.Level.Equals(newError.Level))
                {
                    text += errorLevel;
                }
                else 
                {
                    text += $"**Level:**\r\n```diff\r\n";
                    text += $"- {oldError.Level}\r\n";
                    text += $"+ {newError.Level}\r\n";
                    text += "```\r\n";
                    countChanges++;
                } */
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