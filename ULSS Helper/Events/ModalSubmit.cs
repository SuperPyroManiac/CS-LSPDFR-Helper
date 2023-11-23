using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ModalSubmit
{
    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmitEventArgs e)
    {
        if (e.Interaction.Data.CustomId == "add-plugin")
        {
            UserActionCache cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin
            {
                Name = cache.Plugin.Name,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = cache.Plugin.State,
                Link = plugLink
            };

            plug.DbRowId = Database.AddPlugin(plug);

            await FindPluginMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.CREATE, e: e);
        }
        
        if (e.Interaction.Data.CustomId == "add-error")
        {
            UserActionCache cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            var err = new Error
            {
                Regex = e.Values["errReg"],
                Solution = e.Values["errSol"],
                Description = e.Values["errDesc"],
                Level = cache.Error.Level
            };
            
            if (Database.LoadErrors().Any(error => error.Regex == err.Regex))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This error already exists in the database!\r\nConsider using /EditError <ID>")));
                return;
            }

            err.ID = Database.AddError(err).ToString();

            await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.CREATE, e: e);
        }
        
        if (e.Interaction.Data.CustomId == "edit-plugin")
        {
            UserActionCache cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin
            {
                Name = cache.Plugin.Name,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = cache.Plugin.State,
                Link = plugLink
            };

            Plugin oldPlugin = Database.GetPlugin(plug.Name);

            Database.EditPlugin(plug);

            await FindPluginMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
        }
        
        if (e.Interaction.Data.CustomId == "edit-pluginnotes")
        {
            UserActionCache cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            cache.Plugin.Description = e.Values["plugnotes"];

            Plugin oldPlugin = Database.GetPlugin(cache.Plugin.Name);

            Database.EditPlugin(cache.Plugin);

            await FindPluginMessages.SendDbOperationConfirmation(newPlugin: cache.Plugin, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
        }

        if (e.Interaction.Data.CustomId == "edit-error")
        {
            UserActionCache cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            var err = new Error
            {
                ID = cache.Error.ID,
                Regex = e.Values["errReg"],
                Solution = e.Values["errSol"],
                Description = e.Values["errDesc"],
                Level = cache.Error.Level
            };

            Error oldError = Database.GetError(err.ID);

            Database.EditError(err);

            await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.UPDATE, oldError: oldError, e: e);
        }
        
        if (e.Interaction.Data.CustomId == ComponentInteraction.SendFeedback)
        {
            var feedback = e.Values["feedback"];
            var embed = BasicEmbeds.Generic(
                $"Feedback received!\r\n\r\n"
                    + $"```{feedback}```\r\n\r\n"
                    + $"Sent By:\r\n"
                    + $"<@{e.Interaction.User.Id}> ({e.Interaction.User.Username}) in: <#{e.Interaction.ChannelId}>", 
                DiscordColor.PhthaloGreen
            );
            Logging.SendPubLog(embed);
            await e.Interaction.CreateResponseAsync(
                InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Feedback sent!"))
            );
        }
    }
}