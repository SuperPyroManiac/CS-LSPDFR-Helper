using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Events;

internal class JoinLeave
{
    internal static async Task JoinEvent(DiscordClient s, GuildMemberAddedEventArgs ctx)
    {
        while (!Program.isStarted) await Task.Delay(500);
        var dbUsers = Database.LoadUsers();
        
        
        //Add Users
        if (dbUsers.All(x => x.UID.ToString() != ctx.Member.Id.ToString()))
        {
            var newUser = new DiscordUser
            {
                UID = ctx.Member.Id.ToString(),
                Username = ctx.Member.Username,
                BotEditor = 0,
                BotAdmin = 0,
                Blocked = 0
            };
            Database.AddUser(newUser);
        }
    }
    
    internal static async Task LeaveEvent(DiscordClient s, GuildMemberRemovedEventArgs ctx)
    {
        while (!Program.isStarted) await Task.Delay(500);
        
        //Close case if user leaves.
        await CheckUsers.CloseCases(ctx.Member.Id.ToString());
    }
}