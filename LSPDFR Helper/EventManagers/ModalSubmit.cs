using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.EventManagers;

internal class ModalSubmit
{
    private static readonly List<string> CacheEventIds =
    [
        CustomIds.SelectPluginValueToEdit,
        CustomIds.SelectErrorValueToEdit,
        CustomIds.SelectUserValueToEdit
    ];

    public static async Task HandleModalSubmit(DiscordClient s, ModalSubmittedEventArgs e)
    {
        while (!Program.IsStarted) await Task.Delay(500);

        if ( CacheEventIds.Contains(e.Interaction.Data.CustomId) )
        { //Handle cached modal events here.
            var cache = Program.Cache.GetUserAction(e.Interaction.User.Id, e.Interaction.Data.CustomId);
            
            if (e.Interaction.Data.CustomId == CustomIds.SelectUserValueToEdit)
            {
                var user = new User()
                {
                    UID = cache.User.UID,
                    Username = cache.User.Username,
                    BotEditor = int.Parse(e.Values["userEditor"]),
                    BotAdmin = int.Parse(e.Values["userBotAdmin"]),
                    Blocked = int.Parse(e.Values["userBlacklist"]),
                };
                DbManager.EditUser(user);
                Program.Cache.UpdateUsers(DbManager.GetUsers());

                var embed = BasicEmbeds.Info(
                    $"__User Updated!__\r\n"
                    + $">>> **UID: **{user.UID}\r\n"
                    + $" **Username: **{user.Username}\r\n"
                    + $" **Is Editor: **{Convert.ToBoolean(user.BotEditor)}\r\n"
                    + $" **Is Bot Admin: **{Convert.ToBoolean(user.BotAdmin)}\r\n"
                    + $" **Is Blacklisted: **{Convert.ToBoolean(user.Blocked)}\r\n", true);
                
                await e.Interaction.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed).AsEphemeral());
                
                await Logging.SendLog(e.Interaction.ChannelId, e.Interaction.User.Id, embed);
            }
        }
        //Handle non cached modal events here.
    }
}