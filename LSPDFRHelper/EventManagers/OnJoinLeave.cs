using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.AutoHelper;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.EventManagers;

public static class OnJoinLeave
{
    public static async Task JoinEvent(DiscordClient s, GuildMemberAddedEventArgs ctx)
    {
        try
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
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }

    public static async Task LeaveEvent(DiscordClient s, GuildMemberRemovedEventArgs ctx)
    {
        try
        {
            while ( !Program.IsStarted ) await Task.Delay(500);
        
            //Close case if user leaves
            foreach ( var ac in Program.Cache.GetCases().Where(x => x.Solved == false) )
                if ( ac.OwnerId == ctx.Member.Id ) await CloseCase.Close(ac);
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }

    public static async Task GuildJoinEvent(DiscordClient cl, GuildCreatedEventArgs args)
    {
        try
        {
            while ( !Program.IsStarted ) await Task.Delay(500); 
            var owner = await args.Guild.GetGuildOwnerAsync();
            //var owner = args.Guild.Owner;
            await Logging.ReportPubLog(BasicEmbeds.Info($"__Added To Server__\r\n>>> **Name:** {args.Guild.Name}\r\n**ID:** {args.Guild.Id}\r\n**Members:** {args.Guild.Members.Count}\r\n**Owner:** {owner.Id} ({owner.Username})"));
            await Servers.AddMissing();
            await Servers.RemoveMissing();
            await Servers.Validate();
            _ = Users.Missing();
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
    
    public static async Task GuildLeaveEvent(DiscordClient cl, GuildDeletedEventArgs args)
    {
        try
        {
            while ( !Program.IsStarted ) await Task.Delay(500);

            foreach ( var ac in Program.Cache.GetCases().Where(x => x.ServerId.Equals(args.Guild.Id) && !x.Solved) )
            {
                await CloseCase.Close(ac, true);
            }
            var owner = await args.Guild.GetGuildOwnerAsync();
            //var owner = args.Guild.Owner;
            await Logging.ReportPubLog(BasicEmbeds.Info($"__Removed From Server__\r\n>>> **Name:** {args.Guild.Name}\r\n**ID:** {args.Guild.Id}\r\n**Members:** {args.Guild.Members.Count}\r\n**Owner:** {owner.Id} ({owner.Username})"));
            await Servers.AddMissing();
            await Servers.RemoveMissing();
            await Servers.Validate();
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
}