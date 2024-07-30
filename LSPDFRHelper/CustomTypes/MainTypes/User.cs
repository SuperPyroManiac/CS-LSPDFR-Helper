namespace LSPDFRHelper.CustomTypes.MainTypes;

public class User
{
    public ulong Id { get; set; }
    public string Username { get; set; }
    public bool BotEditor { get; set; }
    public bool BotAdmin { get; set; }
    public bool Blocked { get; set; }
    public string LogPath { get; set; }

    public async Task<bool> IsManager(ulong guild)
    {
        var usr = await Functions.Functions.GetMember(guild, Id);
        return usr.Roles.Any(role => role.Id == Program.Cache.GetServer(guild).ManagerRoleId);
    }
}