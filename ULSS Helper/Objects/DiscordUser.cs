namespace ULSS_Helper.Objects;

public class DiscordUser
{
    public string UID { get; set; }
    public string Username { get; set; }
    public int BotEditor { get; set; }
    public int BotAdmin { get; set; }
    public int Blocked { get; set; }
    public string LogPath { get; set; }

    public async Task<bool> IsTs()
    {
        var usr = await Program.GetMember(UID);
        return usr.Roles.Any(role => role.Id == Program.Settings.Env.TsRoleId);
    }
}