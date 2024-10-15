using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Messages.ModifiedProperties;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.EventManagers;

public static class ModalSubmit
{
    private static readonly List<string> CacheEventIds =
    [
        CustomIds.SelectPluginValueToEdit,
        CustomIds.SelectErrorValueToEdit,
        CustomIds.SelectServerValueToEdit,
        CustomIds.SelectUserValueToEdit
    ];

    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmittedEventArgs e)
    {
        try
        {
            while (!Program.IsStarted) await Task.Delay(500);

            if ( CacheEventIds.Contains(e.Interaction.Data.CustomId) )
            { //Handle cached modal events here.
                var cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
                
                if (e.Interaction.Data.CustomId == CustomIds.SelectPluginValueToEdit)
                {
                    var plugin = cache.Plugin;

                    switch (e.Values.First().Key)
                    {
                        case "Plugin DName":
                            plugin.DName = e.Values["Plugin DName"];
                            break;
                        case "Plugin Version":
                            plugin.Version = e.Values["Plugin Version"];
                            break;
                        case "Plugin EAVersion":
                            plugin.EaVersion = e.Values["Plugin EaVersion"];
                            break;
                        case "Plugin Id":
                            plugin.Id = int.Parse(e.Values["Plugin Id"]);
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
                        $"**Ea Version:** {plugin.EaVersion}\r\n" +
                        $"**Id:** {plugin.Id}\r\n" +
                        $"**Link:** {plugin.Link}\r\n" +
                        $"**Notes:**\r\n" +
                        $"```{plugin.Description}```\r\n" +
                        $"**Type:** {plugin.PluginType}\r\n" +
                        $"**State:** {plugin.State}");
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Current Editor: {e.Interaction.User.Username}",
                        IconUrl = e.Interaction.User.AvatarUrl
                    };
                    bd.AddEmbed(embed);
                    bd.AddComponents(cache.Msg.Components.Cast<DiscordActionRowComponent>());
                
                    var msg = await cache.Msg.ModifyAsync(bd);
                    Program.Cache.SaveUserAction(e.Interaction.User.Id, CustomIds.SelectPluginValueToEdit, new InteractionCache(e.Interaction, plugin, msg));
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                }
                
                if (e.Interaction.Data.CustomId == CustomIds.SelectErrorValueToEdit)
                {
                    var err = cache.Error;

                    switch (e.Values.First().Key)
                    {
                        case "Error Pattern":
                            err.Pattern = e.Values["Error Pattern"];
                            break;
                        case "Error Solution":
                            err.Solution = e.Values["Error Solution"];
                            break;
                        case "Error Description":
                            err.Description = e.Values["Error Description"];
                            break;
                        case "Error String Match":
                            err.StringMatch = e.Values["Error String Match"].Equals("true", StringComparison.OrdinalIgnoreCase);
                            break;
                    }
                    
                    if (err.Id.Equals(0))
                    {
                        if (DbManager.GetErrors().Any(error => error.Pattern == err.Pattern))
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This error already exists!\r\nConsider using /Error edit")));
                            return;
                        }
                        err.Id = DbManager.AddError(err);
                        await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.CREATE, e.Interaction.ChannelId, e.Interaction.User.Id);
                    }
                
                    var bd = new DiscordMessageBuilder();
                    var embed = BasicEmbeds.Info(
                        $"__Editing Error ID: {err.Id}__\r\n" +
                        $">>> **Regex:**\r\n" +
                        $"```{err.Pattern}```\r\n" +
                        $"**Solution:**\r\n" +
                        $"```{err.Solution}```\r\n" +
                        $"**Description:**\r\n" +
                        $"```{err.Description}```\r\n" +
                        $"**String Match:**\r\n" +
                        $"```{err.StringMatch}```\r\n" +
                        $"**Error Level: {err.Level}**");
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Current Editor: {e.Interaction.User.Username}",
                        IconUrl = e.Interaction.User.AvatarUrl
                    };
                    bd.AddEmbed(embed);
                    bd.AddComponents(cache.Msg.Components.Cast<DiscordActionRowComponent>());
                
                    var msg = await cache.Msg.ModifyAsync(bd);
                    Program.Cache.SaveUserAction(e.Interaction.User.Id, CustomIds.SelectErrorValueToEdit, new InteractionCache(e.Interaction, err, msg));
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                }
                
                if (e.Interaction.Data.CustomId == CustomIds.SelectServerValueToEdit)
                {
                    var server = cache.Server;

                    switch (e.Values.First().Key)
                    {
                        case "AhCh":
                            try
                            {
                                server.AutoHelperChId = ulong.Parse(e.Values["AhCh"]);
                                _ = await e.Interaction.Guild.GetChannelAsync(server.AutoHelperChId);
                            }
                            catch ( Exception )
                            { server.AutoHelperChId = 0; }
                            break;
                        case "MonitorCh":
                            try
                            {
                                server.MonitorChId = ulong.Parse(e.Values["MonitorCh"]);
                                _ = await e.Interaction.Guild.GetChannelAsync(server.MonitorChId);
                            }
                            catch ( Exception )
                            { server.MonitorChId = 0; }
                            break;
                        case "ManagerRole":
                            try
                            { server.ManagerRoleId = ulong.Parse(e.Values["ManagerRole"]); }
                            catch ( Exception )
                            { server.ManagerRoleId = 0; }
                            break;
                    }
                
                    var bd = new DiscordMessageBuilder();
                    var embed = BasicEmbeds.Info(
                        $"__Editing Server Settings__\r\n*Leaving a field as '0' will disable it.*\r\n\r\n" +
                        $">>> **AutoHelper Channel Id:**\r\n" +
                        $"<#{server.AutoHelperChId}> \r\n(`{server.AutoHelperChId}`)\r\n" +
                        $"**AutoHelper Monitor Id:**\r\n" +
                        $"<#{server.MonitorChId}> \r\n(`{server.MonitorChId}`)\r\n" +
                        $"**Bot Manager Role Id:**\r\n" +
                        $"`{server.ManagerRoleId}`\r\n");
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Current Editor: {e.Interaction.User.Username}",
                        IconUrl = e.Interaction.User.AvatarUrl
                    };
                    bd.AddEmbed(embed);
                    bd.AddComponents(cache.Msg.Components.Cast<DiscordActionRowComponent>());
                
                    var msg = await cache.Msg.ModifyAsync(bd);
                    Program.Cache.SaveUserAction(e.Interaction.User.Id, CustomIds.SelectServerValueToEdit, new InteractionCache(e.Interaction, server, msg));
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                }
            
                if (e.Interaction.Data.CustomId == CustomIds.SelectUserValueToEdit)
                {
                    var user = cache.User;
                    user.BotEditor = e.Values["userEditor"].Equals("true", StringComparison.OrdinalIgnoreCase);
                    user.BotAdmin = e.Values["userBotAdmin"].Equals("true", StringComparison.OrdinalIgnoreCase);
                    user.Blocked = e.Values["userBlacklist"].Equals("true", StringComparison.OrdinalIgnoreCase);
                    DbManager.EditUser(user);
                    await Task.Delay(250);

                    var embed = BasicEmbeds.Info(
                        $"__User Updated!__\r\n"
                        + $">>> **UID: **{user.Id}\r\n"
                        + $" **Username: **{user.Username}\r\n"
                        + $" **Is Editor: **{user.BotEditor}\r\n"
                        + $" **Is Bot Admin: **{user.BotAdmin}\r\n"
                        + $" **Is Blacklisted: **{user.Blocked}\r\n");
                
                    await e.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
                
                    await Logging.SendLog(e.Interaction.ChannelId, e.Interaction.User.Id, embed);
                }
            }
            //Handle non cached modal events here.

            if (e.Interaction.Data.CustomId == CustomIds.RequestHelp)
            {
                var msg = new DiscordInteractionResponseBuilder();
                msg.IsEphemeral = true;
                var ac = Program.Cache.GetCases().First(x => x.ChannelId.Equals(e.Interaction.Channel.Id));

                if (e.Interaction.User.Id.Equals(ac.OwnerId) || await Program.Cache.GetUser(e.Interaction.User.Id).IsManager(e.Interaction.Guild!.Id))
                {
                    if (!ac.TsRequested)
                    {
                        msg.IsEphemeral = false;
                        msg.AddEmbed(BasicEmbeds.Info(
                            "__Help Requested!__\r\n" +
                            ">>> TS have been sent an alert! " +
                            "Keep in mind they are real people and may not be available at the moment. Patience is key!" +
                            $"\r\n__**Ensure you have explained your issue well!!**__\r\n```{e.Values["issueDsc"]}```"));
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
                        var tsMsg = new DiscordMessageBuilder();
                        tsMsg.AddEmbed(BasicEmbeds.Info(
                            $"__Help Requested! Case: {ac.CaseId}__\r\n" +
                            $">>> Author: <@{ac.OwnerId}> ({Program.Cache.GetUser(ac.OwnerId).Username})\r\n" +
                            $"Thread: <#{ac.ChannelId}>\r\n" +
                            $"Reason:\r\n```{e.Values["issueDsc"]}```\r\n" +
                            $"Created: <t:{e.Interaction.Channel.CreationTimestamp.ToUnixTimeSeconds()}:R>"));
                        tsMsg.AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Secondary, CustomIds.JoinCase,
                            "Join Case", false,
                            new DiscordComponentEmoji("💢")), new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.IgnoreRequest,
                            "Ignore", false,
                            new DiscordComponentEmoji("🪠")));
                        var tsMsgSent = await Program.Client.GetChannelAsync(Program.Cache.GetServer(e.Interaction.Guild!.Id).MonitorChId);
                        var rCh = await tsMsgSent.SendMessageAsync(tsMsg);
                        ac.TsRequested = true;
                        ac.RequestId = rCh.Id;
                        ac.ExpireDate = DateTime.Now.ToUniversalTime().AddHours(12);
                        DbManager.EditCase(ac);
                        await AutoHelper.UpdateAhMonitor(e.Interaction.Guild.Id);
                        return;
                    }
                    if (ac.TsRequested)
                    {
                        msg.AddEmbed(BasicEmbeds.Error("Help has already been requested!"));
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
                        return;
                    }
                }
                msg.AddEmbed(BasicEmbeds.Error("You do not own this case!"));
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, msg);
            }
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}