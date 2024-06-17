namespace LSPDFR_Helper.CustomTypes.MainTypes;

internal class User
{
    internal ulong Id { get; set; }
    internal string Username { get; set; }
    internal bool BotEditor { get; set; }
    internal bool BotAdmin { get; set; }
    internal bool Blocked { get; set; }
    internal string LogPath { get; set; }

    internal async Task<bool> IsTs()
    {
        var usr = await Functions.Functions.GetMember(Id);
        return usr.Roles.Any(role => role.Id == Program.Settings.TsRoleId);
    }
}