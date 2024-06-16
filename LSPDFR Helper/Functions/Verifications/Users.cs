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
            if (DbUsers.All(x => x.Id != user.Id))
            {
                if (user == null) continue;
                cnt++;

                var newUser = new User()
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
        await Task.Delay(250);
        if (cnt > 0) Program.Cache.UpdateUsers(DbManager.GetUsers());
        return cnt;
    }
    
    internal static async Task<int> Usernames()
    {
        var cnt = 0;
        foreach (var user in DbUsers)
        {
            if (!ServerUsers.ContainsKey(user.Id)) continue;
            if (ServerUsers[user.Id].Username != user.Username)
            {
                cnt++;
                user.Username = ServerUsers[user.Id].Username;
                await Task.Delay(100);
                DbManager.EditUser(user);
            }
        }
        await Task.Delay(250);
        if (cnt > 0) Program.Cache.UpdateUsers(DbManager.GetUsers());
        return cnt;
    }
}