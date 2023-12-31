using DSharpPlus.SlashCommands;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class ImportUsers
{
    [SlashCommand("ImportUsers", "Imports All Users!")]
    [RequireBotAdmin]
    public async Task ImportUsersCmd(InteractionContext ctx)
    {
        await foreach (var user in ctx.Guild.GetAllMembersAsync())
        {
            if (Database.LoadUsers().All(us => us.UID != user.Id.ToString()))
            {
                var newUser = new DiscordUser()
                {
                    UID = user.Id.ToString(),
                    Username = user.Username,
                    TS = 0,
                    View = 0,
                    Editor = 0,
                    BotAdmin = 0,
                    Bully = 0
                };
                Database.AddUser(newUser);
            }
        }
    }
}