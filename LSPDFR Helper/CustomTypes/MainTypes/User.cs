using LSPDFR_Helper.CustomTypes.SpecialTypes;

namespace LSPDFR_Helper.CustomTypes.MainTypes;
using LSPDFR_Helper.Functions;

internal class User
{
    public string UID { get; set; }
    public string Username { get; set; }
    public int BotEditor { get; set; }
    public int BotAdmin { get; set; }
    public int Blocked { get; set; }
    public string LogPath { get; set; }

    public async Task<bool> IsTs()
    {
        var usr = await Functions.GetMember(UID);
        return usr.Roles.Any(role => role.Id == Program.Settings.TsRoleId);
    }
}