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
    public const string EditError = ComponentInteraction.SelectErrorValueToEdit;
    public const string EditUser = "edit-user";
    public const string SendFeedback = ComponentInteraction.SendFeedback;
    public const string RequestHelp = ComponentInteraction.RequestHelp;

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
                if (string.IsNullOrEmpty(plugVersion))
                    plugVersion = null;
                var plugEaVersion = e.Values["plugEAVersion"];
                if (string.IsNullOrEmpty(plugEaVersion))
                    plugEaVersion = null;
                var plugId = e.Values["plugID"];
                if (string.IsNullOrEmpty(plugId))
                    plugId = null;
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
                await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));
                
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
                if (string.IsNullOrEmpty(plugVersion))
                    plugVersion = null;
                var plugEaVersion = e.Values["plugEAVersion"];
                if (string.IsNullOrEmpty(plugEaVersion))
                    plugEaVersion = null;
                var plugId = e.Values["plugID"];
                if (string.IsNullOrEmpty(plugId))
                    plugId = null;
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
                await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));

                await FindPluginMessages.SendDbOperationConfirmation(newPlugin: plug, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
            }
            
            if (e.Interaction.Data.CustomId == EditPluginNotes)
            {
                cache.Plugin.Description = e.Values["plugnotes"];

                var oldPlugin = Database.GetPlugin(cache.Plugin.Name);

                Database.EditPlugin(cache.Plugin);
                await Task.Run(() => Program.Cache.UpdatePlugins(Database.LoadPlugins()));

                await FindPluginMessages.SendDbOperationConfirmation(newPlugin: cache.Plugin, operation: DbOperation.UPDATE, oldPlugin: oldPlugin, e: e);
            }

            if (e.Interaction.Data.CustomId == EditError)
            {
                var err = cache.Error;
                var oldErr = Database.GetError(err.ID);

                switch (e.Values.First().Key)
                {
                    case "Error Regex":
                        err.Regex = e.Values["Error Regex"];
                        break;
                    case "Error Solution":
                        err.Solution = e.Values["Error Solution"];
                        break;
                    case "Error Description":
                        err.Description = e.Values["Error Description"];
                        break;
                }

                Database.EditError(err);
                
                var bd = new DiscordMessageBuilder();
                var embed = BasicEmbeds.Info(
                    $"__Editing Error ID: {err.ID}__\r\n" +
                    $">>> **Regex:**\r\n" +
                    $"```{err.Regex}```\r\n" +
                    $"**Solution:**\r\n" +
                    $"```{err.Solution}```\r\n" +
                    $"**Description:**\r\n" +
                    $"```{err.Description}```\r\n" +
                    $"**Error Level: {err.Level}**", true);
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = e.Interaction.User.Username,
                    IconUrl = e.Interaction.User.AvatarUrl
                };
                bd.AddEmbed(embed);
                bd.AddComponents(cache.Msg.Components);
                
                var msg = await cache.Msg.ModifyAsync(bd);
                await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.UPDATE, oldError: oldErr, e: e);

                Program.Cache.RemoveUserAction(e.Interaction.User.Id, ComponentInteraction.SelectErrorValueToEdit);
                Program.Cache.SaveUserAction(e.Interaction.User.Id, ComponentInteraction.SelectErrorValueToEdit, new UserActionCache(e.Interaction, err, msg));
                
                
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Success("Value updated!")));
                await e.Interaction.DeleteOriginalResponseAsync();
            }
            
            if (e.Interaction.Data.CustomId == EditUser)
            {
                var user = new DiscordUser()
                {
                    UID = cache.User.UID,
                    Username = cache.User.Username,
                    BotEditor = int.Parse(e.Values["userEditor"]),
                    BotAdmin = int.Parse(e.Values["userBotAdmin"]),
                    Blocked = int.Parse(e.Values["userBlacklist"]),
                    Bully = int.Parse(e.Values["userBully"])
                };
                Database.EditUser(user);

                var embed = BasicEmbeds.Info(
                    $"__User Updated!__\r\n"
                    + $">>> **UID: **{user.UID}\r\n"
                    + $" **Username: **{user.Username}\r\n"
                    + $" **Is Editor: **{user.BotEditor}\r\n"
                    + $" **Is Bot Admin: **{user.BotAdmin}\r\n"
                    + $" **Is Blacklisted: **{user.Blocked}\r\n"
                    + $" **Bully Victim: **{user.Bully}\r\n", true);
                
                await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral(true));
                
                Logging.SendLog(e.Interaction.ChannelId,e.Interaction.User.Id,embed);
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
					InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Feedback sent!")).AsEphemeral(true)
					);
			}
            
            if (e.Interaction.Data.CustomId == RequestHelp)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Database.LoadCases().First(x => x.ChannelID.Equals(e.Interaction.Channel.Id.ToString()));

                if (e.Interaction.User.Id.ToString().Equals(ac.OwnerID) || e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id)
                        .Result.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
                {
                    if (ac.TsRequested == 0)
                    {
                        msg.IsEphemeral = false;
                        msg.AddEmbed(BasicEmbeds.Info(
                            "__Help Requested!__\r\n" +
                            ">>> TS have been sent an alert! " +
                            "Keep in mind they are real people and may not be available at the moment. Patience is key!" +
                            $"\r\n__**Ensure you have explained your issue well!!**__\r\n```{e.Values["issueDsc"]}```", true));
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
                        var tsMsg = new DiscordMessageBuilder();
                        tsMsg.AddEmbed(BasicEmbeds.Info(
                            $"__Help Requested! Case: {ac.CaseID}__\r\n" +
                            $">>> Author: <@{ac.OwnerID}> ({e.Interaction.Guild.GetMemberAsync(ulong.Parse(ac.OwnerID)).Result.DisplayName})\r\n" +
                            $"Thread: <#{ac.ChannelID}>\r\n" +
                            $"Reason:\r\n```{e.Values["issueDsc"]}```\r\n" +
                            $"Created: <t:{e.Interaction.Channel.CreationTimestamp.ToUnixTimeSeconds()}:R>", true));
                        tsMsg.AddComponents([
                            new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.JoinCase, "Join Case", false,
                            new DiscordComponentEmoji("💢"))]);
                        var tsMsgSent = await e.Interaction.Guild.GetChannel(Program.Settings.Env.RequestHelpChannelId)
                            .SendMessageAsync(tsMsg);
                        ac.TsRequested = 1;
                        ac.RequestID = tsMsgSent.Id.ToString();
                        ac.Timer = 24;
                        Database.EditCase(ac);
                        return;
                    }
                    if (ac.TsRequested == 1)
                    {
                        msg.AddEmbed(BasicEmbeds.Error("Help has already been requested!"));
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
                        return;
                    }
                }
                msg.AddEmbed(BasicEmbeds.Error("You do not own this case!"));
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, msg);
            }
		}
    }
}