using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.Functions.Verifications;

internal class Users
{
    private static readonly IReadOnlyDictionary<ulong, DiscordMember> ServerUsers = Functions.GetGuild().Members;
    private static readonly List<User> DbUsers = DbManager.GetUsers();
    
    internal static async Task<int> Missing()
    {
        var cnt = 0;
        foreach (var user in ServerUsers.Values.ToList())
        {
            if (DbUsers.All(x => x.UID.ToString() != user.Id.ToString()))
            {
                if (user == null) continue;
                cnt++;

                var newUser = new User()
                {
                    UID = user.Id,
                    Username = user.Username,
                    BotEditor = 0,
                    BotAdmin = 0,
                    Blocked = 0
                };
                await Task.Delay(100);
                DbManager.AddUser(newUser);
            }
        }
        if (cnt > 0) Program.Cache.UpdateUsers(DbManager.GetUsers());
        return cnt;
    }
    
    internal static async Task<int> Usernames()
    {
        var cnt = 0;
        foreach (var user in DbUsers)
        {
            if (!ServerUsers.ContainsKey(user.UID)) continue;
            if (ServerUsers[user.UID].Username != user.Username)
            {
                cnt++;
                user.Username = ServerUsers[user.UID].Username;
                await Task.Delay(100);
                DbManager.EditUser(user);
            }
        }
        if (cnt > 0) Program.Cache.UpdateUsers(DbManager.GetUsers());
        return cnt;
    }
}