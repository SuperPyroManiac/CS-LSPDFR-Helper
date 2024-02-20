using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Events;

internal class JoinLeave
{
    internal static async Task JoinEvent(DiscordClient s, GuildMemberAddEventArgs ctx)
    {
        var dbUsers = Database.LoadUsers();
        
        //Add Users
        if (dbUsers.All(x => x.UID.ToString() != ctx.Member.Id.ToString()))
        {
            var newUser = new DiscordUser()
            {
                UID = ctx.Member.Id.ToString(),
                Username = ctx.Member.Username,
                TS = 0,
                View = 0,
                Editor = 0,
                BotAdmin = 0,
                Bully = 0,
                Blocked = 0
            };
            Database.AddUser(newUser);
        }
    }
    
    internal static async Task LeaveEvent(DiscordClient s, GuildMemberRemoveEventArgs ctx)
    {
        await CheckUsers.CloseCases(ctx.Member.Id.ToString());
    }
}