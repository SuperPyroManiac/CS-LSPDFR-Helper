using LSPDFRHelper.CustomTypes.MainTypes;

namespace LSPDFRHelper.Functions.Verifications;

public static class Users
{
    
    public static async Task<int> Missing()
    {
        var serverUsers = Functions.GetGuild().Members;
        var dbUsers = DbManager.GetUsers();
        
        var cnt = 0;
        foreach (var user in serverUsers.Values.ToList())
        {
            if (dbUsers.All(x => x.Id != user.Id))
            {
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
        }
        return cnt;
    }
    
    public static async Task<int> Usernames()
    {
        var serverUsers = Functions.GetGuild().Members;
        var dbUsers = DbManager.GetUsers();
        
        var cnt = 0;
        foreach (var user in dbUsers)
        {
            if (!serverUsers.ContainsKey(user.Id)) continue;
            if (serverUsers[user.Id].DisplayName != user.Username)
            {
                cnt++;
                user.Username = serverUsers[user.Id].DisplayName;
                DbManager.EditUser(user);
            }
        }
        return cnt;
    }
}