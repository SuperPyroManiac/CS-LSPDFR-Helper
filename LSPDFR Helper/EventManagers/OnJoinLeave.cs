using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions;

namespace LSPDFR_Helper.EventManagers;

internal static class OnJoinLeave
{
    internal static async Task JoinEvent(DiscordClient s, GuildMemberAddedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        //Add Missing Users
        if (Program.Cache.GetUsers().All(x => x.Id.ToString() != ctx.Member.Id.ToString()))
        {
            var newUser = new User
            {
                Id = ctx.Member.Id,
                Username = ctx.Member.Username,
                BotEditor = false,
                BotAdmin = false,
                Blocked = false
            };
            DbManager.AddUser(newUser);
        }
    }

    internal static async Task LeaveEvent(DiscordClient s, GuildMemberRemovedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        //TODO: CloseCase on leave
    }
}