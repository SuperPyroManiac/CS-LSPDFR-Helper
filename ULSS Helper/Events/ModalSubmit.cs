using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Events;

public class ModalSubmit
{
    public const string AddPlugin = "add-plugin";
    public const string AddError = "add-error";
    public const string EditPlugin = "edit-plugin";
    public const string EditPluginNotes = "edit-pluginnotes";
    public const string EditError = "edit-error";
    public const string EditUser = "edit-user";
    public const string SendFeedback = ComponentInteraction.SendFeedback;

    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmitEventArgs e)
    {
        List<string> cacheEventIds =
        [
            AddPlugin,
            AddError,
            EditPlugin,
            EditPluginNotes,
            EditError,
            EditUser
        ];

        if (cacheEventIds.Contains(e.Interaction.Data.CustomId))
        {
            var cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
        
            if (e.Interaction.Data.CustomId == AddPlugin)
            {
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
                    Description = "N/A",
                    State = cache.Plugin.State,
                    Link = plugLink
                };
                
                if (Database.LoadPlugins().Any(plugin => plugin.Name == plug.Name))
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This plugin already exists in the database!\r\nConsider using /EditPlugin <Name>")));
                    return;
                }

                Database.AddPlugin(plug);
                
                await FindPluginMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.CREATE, e: e);
            }
            
            if (e.Interaction.Data.CustomId == AddError)
            {
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
            
            if (e.Interaction.Data.CustomId == EditPlugin)
            {
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
                    Description = cache.Plugin.Description,
                    State = cache.Plugin.State,
                    Link = plugLink
                };

                var oldPlugin = Database.GetPlugin(plug.Name);

                Database.EditPlugin(plug);

                await FindPluginMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
            }
            
            if (e.Interaction.Data.CustomId == EditPluginNotes)
            {
                cache.Plugin.Description = e.Values["plugnotes"];

                var oldPlugin = Database.GetPlugin(cache.Plugin.Name);

                Database.EditPlugin(cache.Plugin);

                await FindPluginMessages.SendDbOperationConfirmation(newPlugin: cache.Plugin, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
            }

            if (e.Interaction.Data.CustomId == EditError)
            {
                var err = new Error
                {
                    ID = cache.Error.ID,
                    Regex = e.Values["errReg"],
                    Solution = e.Values["errSol"],
                    Description = e.Values["errDesc"],
                    Level = cache.Error.Level
                };

                var oldError = Database.GetError(err.ID);

                Database.EditError(err);

                await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.UPDATE, oldError: oldError, e: e);
            }
            
            if (e.Interaction.Data.CustomId == EditUser)
            {
                var user = new DiscordUser()
                {
                    UID = cache.User.UID,
                    Username = cache.User.Username,
                    TS = int.Parse(e.Values["userTs"]),
                    View = int.Parse(e.Values["userView"]),
                    Editor = int.Parse(e.Values["userEditor"]),
                    BotAdmin = int.Parse(e.Values["userBotAdmin"]),
                    Bully = int.Parse(e.Values["userBully"])
                };
                Database.EditUser(user);

                var embed = BasicEmbeds.Info(
                    $"__User Updated!__\r\n"
                    + $">>> **UID: **{user.UID}\r\n"
                    + $">>> **Username: **{user.Username}\r\n"
                    + $">>> **Is TS: **{user.TS}\r\n"
                    + $">>> **Xtra View: **{user.View}\r\n"
                    + $">>> **Is Editor: **{user.Editor}\r\n"
                    + $">>> **Is Bot Admin: **{user.BotAdmin}\r\n"
                    + $">>> **Bully Victim: **{user.Bully}\r\n");
                Logging.SendLog(e.Interaction.ChannelId,e.Interaction.User.Id,embed);
                await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Feedback sent!")));
            }
            
			// delete the cached data of the action that is completed now (which means the cache isn't needed anymore)
            Program.Cache.RemoveUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
        }
		else // modal submit events that don't require cached data
		{
			if (e.Interaction.Data.CustomId == SendFeedback)
			{
				var feedback = e.Values["feedback"];
				var embed = BasicEmbeds.Generic(
					$"### :grey_exclamation: __Feedback received!__\r\n"
					+ $">>> ```{feedback}```\r\n"
					+ $"Sent By:\r\n"
					+ $"<@{e.Interaction.User.Id}> ({e.Interaction.User.Username}) in: <#{e.Interaction.ChannelId}>",
					DiscordColor.SapGreen
					);
				Logging.SendPubLog(embed);
				await e.Interaction.CreateResponseAsync(
					InteractionResponseType.UpdateMessage,
					new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Feedback sent!"))
					);
			}
		}
    }
}