namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class User
{
    public ulong Id { get; set; }
    public string Username { get; set; }
    public bool BotEditor { get; set; }
    public bool BotAdmin { get; set; }
    public bool Blocked { get; set; }
    public string LogPath { get; set; }

    public async Task<bool> IsTs()
    {
        var usr = await Functions.Functions.GetMember(Id);
        return usr.Roles.Any(role => role.Id == Program.Settings.TsRoleId);
    }
}