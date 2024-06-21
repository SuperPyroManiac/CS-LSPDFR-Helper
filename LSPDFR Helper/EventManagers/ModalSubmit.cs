using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.EventManagers;

public static class ModalSubmit
{
    private static readonly List<string> CacheEventIds =
    [
        CustomIds.SelectPluginValueToEdit,
        CustomIds.SelectErrorValueToEdit,
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
                    if (DbManager.GetPlugins().Any(p => p.Name != plugin.Name))
                    {
                        DbManager.AddPlugin(plugin);
                        //await FindPluginMessages.SendDbOperationConfirmation(plugin, operation: DbOperation.CREATE, e.Interaction.ChannelId, e.Interaction.User.Id);
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
                            plugin.EaVersion = e.Values["Plugin EaVersion"];
                            break;
                        case "Plugin ID":
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
                        $"__Adding New Plugin: {plugin.Name}__\r\n>>> " +
                        $"**Display Name:** {plugin.DName}\r\n" +
                        $"**Version:** {plugin.Version}\r\n" +
                        $"**Ea Version:** {plugin.EaVersion}\r\n" +
                        $"**Id:** {plugin.Id}\r\n" +
                        $"**Link:** {plugin.Link}\r\n" +
                        $"**Notes:**\r\n" +
                        $"```{plugin.Description}```\r\n" +
                        $"**Type:** {plugin.PluginType}\r\n" +
                        $"**State:** {plugin.State}", true);
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Current Editor: {e.Interaction.User.Username}",
                        IconUrl = e.Interaction.User.AvatarUrl
                    };
                    bd.AddEmbed(embed);
                    bd.AddComponents(cache.Msg.Components!);
                
                    var msg = await cache.Msg.ModifyAsync(bd);
                    Program.Cache.SaveUserAction(e.Interaction.User.Id, CustomIds.SelectPluginValueToEdit, new InteractionCache(e.Interaction, plugin, msg));
                    await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                }
                
                if (e.Interaction.Data.CustomId == CustomIds.SelectErrorValueToEdit)
                {
                    var err = cache.Error;
                    if (string.IsNullOrEmpty(err.Id.ToString()))
                    {
                        if (DbManager.GetErrors().Any(error => error.Pattern == err.Pattern))
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Error("This error already exists!\r\nConsider using /EditError <ID>", true)));
                            return;
                        }
                        err.Id = DbManager.AddError(err);
                        //await FindErrorMessages.SendDbOperationConfirmation(newError: err, operation: DbOperation.CREATE, e.Interaction.ChannelId, e.Interaction.User.Id);
                    }

                    switch (e.Values.First().Key)
                    {
                        case "Error Regex":
                            err.Pattern = e.Values["Error Regex"];
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
                        $"**Error Level: {err.Level}**", true);
                    embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Current Editor: {e.Interaction.User.Username}",
                        IconUrl = e.Interaction.User.AvatarUrl
                    };
                    bd.AddEmbed(embed);
                    bd.AddComponents(cache.Msg.Components!);
                
                    var msg = await cache.Msg.ModifyAsync(bd);
                    Program.Cache.SaveUserAction(e.Interaction.User.Id, CustomIds.SelectErrorValueToEdit, new InteractionCache(e.Interaction, err, msg));
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
                    Program.Cache.UpdateUsers(DbManager.GetUsers());

                    var embed = BasicEmbeds.Info(
                        $"__User Updated!__\r\n"
                        + $">>> **UID: **{user.Id}\r\n"
                        + $" **Username: **{user.Username}\r\n"
                        + $" **Is Editor: **{user.BotEditor}\r\n"
                        + $" **Is Bot Admin: **{user.BotAdmin}\r\n"
                        + $" **Is Blacklisted: **{user.Blocked}\r\n", true);
                
                    await e.Interaction.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
                
                    await Logging.SendLog(e.Interaction.ChannelId, e.Interaction.User.Id, embed);
                }
            }
            //Handle non cached modal events here.
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}