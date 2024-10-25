using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public static class Users
{
    
    public static async Task<int> Missing()
    {
        try
        {
            var serverUsers = new List<DiscordUser>();
            
            foreach ( var srv in Program.Client.Guilds.Values )
            foreach ( var usr in srv.Members )
                if (!serverUsers.Contains(usr.Value)) serverUsers.Add(usr.Value);
        
            var dbUsers = DbManager.GetUsers();
        
            var cnt = 0;
            foreach (var user in serverUsers)
            {
                if ( dbUsers.Any(x => x.Id == user.Id) ) continue;
                if (user == null) continue;
                cnt++;

                var newUser = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    BotEditor = false,
                    BotAdmin = false,
                    Blocked = false
                };
                await Task.Delay(100);
                DbManager.AddUser(newUser);
            }
            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }
    
    public static async Task<int> Usernames()
    {
        try
        {
            var cnt = 0;
            foreach ( var srv in Program.Client.Guilds.Values )
            {
                var dbUsers = DbManager.GetUsers();
            
                foreach (var user in dbUsers)
                {
                    if (!srv.Members.ContainsKey(user.Id)) continue;
                    if (srv.Members[user.Id].Username != user.Username)
                    {
                        cnt++;
                        user.Username = srv.Members[user.Id].Username;
                        await Task.Delay(100);
                        DbManager.EditUser(user);
                    }
                }
            }
            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }
}