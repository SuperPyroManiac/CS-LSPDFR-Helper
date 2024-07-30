using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.AutoHelper;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.EventManagers;

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

    public static async Task GuildJoinEvent(DiscordClient cl, GuildCreatedEventArgs args)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        var owner = await args.Guild.GetGuildOwnerAsync();
        await Logging.ReportPubLog(BasicEmbeds.Info($"__Added To Server__\r\n>>> **Name:** {args.Guild.Name}\r\n**ID:** {args.Guild.Id}\r\n**Owner:** {owner.Id} ({owner.Username})"));
        await Functions.Verifications.Servers.AddMissing();
        await Functions.Verifications.Servers.RemoveMissing();
        await Functions.Verifications.Servers.Validate();
    }
    
    public static async Task GuildLeaveEvent(DiscordClient cl, GuildDeletedEventArgs args)
    {
        while ( !Program.IsStarted ) await Task.Delay(500);
        
        var owner = await args.Guild.GetGuildOwnerAsync();
        await Logging.ReportPubLog(BasicEmbeds.Info($"__Removed From Server__\r\n>>> **Name:** {args.Guild.Name}\r\n**ID:** {args.Guild.Id}\r\n**Owner:** {owner.Id} ({owner.Username})"));
        await Functions.Verifications.Servers.AddMissing();
        await Functions.Verifications.Servers.RemoveMissing();
        await Functions.Verifications.Servers.Validate();
    }
}