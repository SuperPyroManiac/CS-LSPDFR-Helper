using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.AutoHelper;

namespace LSPDFR_Helper.EventManagers;

public static class OnJoinLeave
{
    public static async Task JoinEvent(DiscordClient s, GuildMemberAddedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        //Add Missing Users
        if (Program.Cache.GetUsers().All(x => x.Id.ToString() != ctx.Member.Id.ToString()))
        {
            var newUser = new User
            {
                Id = ctx.Member.Id,
                Username = ctx.Member.DisplayName,
                BotEditor = false,
                BotAdmin = false,
                Blocked = false
            };
            DbManager.AddUser(newUser);
        }
    }

    public static async Task LeaveEvent(DiscordClient s, GuildMemberRemovedEventArgs ctx)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        //Close case if user leaves
        foreach ( var ac in Program.Cache.GetCases().Where(x => x.Solved == false) )
            if ( ac.OwnerId == ctx.Member.Id ) await CloseCase.Close(ac);
        
    }
}