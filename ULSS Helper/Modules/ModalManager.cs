using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Modules;

public class ModalManager
{
        public static async Task PluginModal(DiscordClient s, ModalSubmitEventArgs e)
    {
        if (e.Interaction.Data.CustomId == "add-plugin")
        {
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin()
            {
                Name = Program.PlugName,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = Program.PlugState.ToString().ToUpper(),
                Link = plugLink
            };

            long dbRowId = DatabaseManager.AddPlugin(plug);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Added {Program.PlugName}!**\r\n"
                    + $"DB Row ID: {dbRowId}\r\n"
                    + $"Display Name: {plugDName}\r\n" 
                    + $"Version: {plugVersion}\r\n"
                    + $"Early Access Version: {plugEaVersion}\r\n"
                    + $"ID (on lcpdfr.com): {plugId}\r\n"
                    + $"Link: {plugLink}\r\n"
                    + $"State: {Program.PlugState}"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "add-error")
        {
            var err = new Error()
            {
                Regex = e.Values["errReg"],
                Solution = e.Values["errSol"],
                Level = Program.ErrLevel.ToString().ToUpper()
            };
            
            if (DatabaseManager.LoadErrors().Any(error => error.Regex == err.Regex))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Error("This error already exists in the database!\r\nConsider using /EditError <ID>")));
                return;
            }

            long dbRowId = DatabaseManager.AddError(err);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Added a {err.Level} error with ID {dbRowId}**\r\n"
                    + $"Regex:\r\n```{err.Regex}```\r\n" 
                    + $"Solution:\r\n```{err.Solution}```"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "edit-plugin")
        {
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin()
            {
                Name = Program.PlugName,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = Program.PlugState.ToString().ToUpper(),
                Link = plugLink
            };

            DatabaseManager.EditPlugin(plug);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Modified {Program.PlugName}!**\r\n"
                    + $"Display Name: {plugDName}\r\n"
                    + $"Version: {plugVersion}\r\n"
                    + $"Early Access Version: {plugEaVersion}\r\n"
                    + $"ID (on lcpdfr.com): {plugId}\r\n"
                    + $"Link: {plugLink}\r\n"
                    + $"State: {Program.PlugState}"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "edit-error")
        {
            var errReg = e.Values["errReg"];
            var errSol = e.Values["errSol"];

            var err = new Error()
            {
                ID = Program.ErrId,
                Regex = errReg,
                Solution = errSol,
                Level = Program.ErrLevel.ToString()
            };

            DatabaseManager.EditError(err);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Modified error ID: {Program.ErrId}!**\r\n"
                    + $"Regex: {errReg}\r\n"
                    + $"Solution: {errSol}\r\n"
                    + $"Level: {Program.ErrLevel.ToString()}"
                ))
            );
        }
    }
}