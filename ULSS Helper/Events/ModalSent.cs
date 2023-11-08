using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ModalSent
{
    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmitEventArgs e)
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

            plug.DbRowId = Database.AddPlugin(plug);

            await PluginCmdMessages.SendDbOperationConfirmation(plug, DbOperation.CREATE);
        }
        
        if (e.Interaction.Data.CustomId == "add-error")
        {
            var err = new Error()
            {
                Regex = e.Values["errReg"],
                Solution = e.Values["errSol"],
                Level = Program.ErrLevel.ToString().ToUpper()
            };
            
            if (Database.LoadErrors().Any(error => error.Regex == err.Regex))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This error already exists in the database!\r\nConsider using /EditError <ID>")));
                return;
            }

            err.ID = Database.AddError(err).ToString();

            await ErrorCmdMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.CREATE, e: e);
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

            Plugin? oldPlugin = Database.GetPlugin(plug.Name);

            Database.EditPlugin(plug);

            await PluginCmdMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
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

            Error? previousError = Database.GetError(err.ID);

            Database.EditError(err);

            await ErrorCmdMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.UPDATE, oldError: previousError, e: e);
        }
    }
}