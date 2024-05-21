using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using DiscordUser = ULSS_Helper.Objects.DiscordUser;

namespace ULSS_Helper.Events;

public class ModalSubmit
{
    public const string EditPlugin = ComponentInteraction.SelectPluginValueToEdit;
    public const string EditError = ComponentInteraction.SelectErrorValueToEdit;
    public const string EditUser = "edit-user";
    public const string SendFeedback = ComponentInteraction.SendFeedback;
    public const string RequestHelp = ComponentInteraction.RequestHelp;

    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmitEventArgs e)
    {
        List<string> cacheEventIds =
        [
            EditPlugin,
            EditError,
            EditUser
        ];

        if (cacheEventIds.Contains(e.Interaction.Data.CustomId))
        {
            var cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            
            if (e.Interaction.Data.CustomId == EditPlugin)
            {
                var plugin = cache.Plugin;
                if (Database.GetPlugin(plugin.Name) == null)
                {
                    Database.AddPlugin(plugin);
                    await FindPluginMessages.SendDbOperationConfirmation(plugin, operation: DbOperation.CREATE, e.Interaction.ChannelId, e.Interaction.User.Id);
                }

                switch (e.Values.First().Key)
                {
                    case "Plugin DName":
                        plugin.DName = e.Values["Plugin DName"];
                        break;
                    case "Plugin Version":
                        plugin.Version = e.Values["Plugin Version"];
                        break;
                    case "Plugin EAVersion":
                        plugin.EAVersion = e.Values["Plugin EAVersion"];
                        break;
                    case "Plugin ID":
                        plugin.ID = e.Values["Plugin ID"];
                        break;
                    case "Plugin Link":
                        plugin.Link = e.Values["Plugin Link"];
                        break;
                    case "Plugin Notes":
                        plugin.Description = e.Values["Plugin Notes"];
                        break;
                }
                
                var bd = new DiscordMessageBuilder();
                var embed = BasicEmbeds.Info(
                    $"__Editing Plugin: {plugin.Name}__\r\n>>> " +
                    $"**Display Name:** {plugin.DName}\r\n" +
                    $"**Version:** {plugin.Version}\r\n" +
                    $"**EA Version:** {plugin.EAVersion}\r\n" +
                    $"**ID:** {plugin.ID}\r\n" +
                    $"**Link:** {plugin.Link}\r\n" +
                    $"**Notes:**\r\n" +
                    $"```{plugin.Description}```\r\n" +
                    $"**State:** {plugin.State}", true);
                embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Current Editor: {e.Interaction.User.Username}",
                    IconUrl = e.Interaction.User.AvatarUrl
                };
                bd.AddEmbed(embed);
                bd.AddComponents(cache.Msg.Components);
                
                var msg = await cache.Msg.ModifyAsync(bd);
                Program.Cache.SaveUserAction(e.Interaction.User.Id, ComponentInteraction.SelectPluginValueToEdit, new UserActionCache(e.Interaction, plugin, msg));
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            }

            if (e.Interaction.Data.CustomId == EditError)
            {
                var err = cache.Error;
                if (string.IsNullOrEmpty(err.ID))
                {
                    if (Database.LoadErrors().Any(error => error.Regex == err.Regex))
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This error already exists!\r\nConsider using /EditError <ID>", true)));
                        return;
                    }
                    err.ID = Database.AddError(err).ToString();
                    await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.CREATE, e.Interaction.ChannelId, e.Interaction.User.Id);
                }

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
                    Text = $"Current Editor: {e.Interaction.User.Username}",
                    IconUrl = e.Interaction.User.AvatarUrl
                };
                bd.AddEmbed(embed);
                bd.AddComponents(cache.Msg.Components);
                
                var msg = await cache.Msg.ModifyAsync(bd);
                Program.Cache.SaveUserAction(e.Interaction.User.Id, ComponentInteraction.SelectErrorValueToEdit, new UserActionCache(e.Interaction, err, msg));
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
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
                
                await Logging.SendLog(e.Interaction.ChannelId, e.Interaction.User.Id, embed);
            }
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
					+ $"User: <@{e.Interaction.User.Id}> ({e.Interaction.User.Username})\r\n" 
                    + $"Channel: {e.Interaction.Channel.Mention}",
					DiscordColor.SapGreen
					);
				await Logging.SendPubLog(embed);
				await e.Interaction.CreateResponseAsync(
					InteractionResponseType.ChannelMessageWithSource,
					new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Feedback sent!")).AsEphemeral(true)
					);
			}
            
            if (e.Interaction.Data.CustomId == RequestHelp)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Program.Cache.GetCasess().First(x => x.ChannelID.Equals(e.Interaction.Channel.Id.ToString()));
                var tmpusr = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                if (e.Interaction.User.Id.ToString().Equals(ac.OwnerID) || tmpusr.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId))
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
                        var tmpownr = await e.Interaction.Guild.GetMemberAsync(ulong.Parse(ac.OwnerID));
                        tsMsg.AddEmbed(BasicEmbeds.Info(
                            $"__Help Requested! Case: {ac.CaseID}__\r\n" +
                            $">>> Author: <@{ac.OwnerID}> ({tmpownr.DisplayName})\r\n" +
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