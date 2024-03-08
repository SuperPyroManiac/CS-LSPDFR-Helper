namespace ULSS_Helper.Objects;

public class DiscordUser
{
    public string UID { get; set; }
    public string Username { get; set; }
    public int BotEditor { get; set; }
    public int BotAdmin { get; set; }
    public int Bully { get; set; }
    public int Blocked { get; set; }
}