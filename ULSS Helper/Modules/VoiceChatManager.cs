using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Modules;

public class VoiceChatManager
{
    public static async Task OnMemberJoinLeaveVC(DiscordClient s, VoiceStateUpdateEventArgs ctx)
    {
        if ((ctx.Channel.UserPermissions & Permissions.SendMessages) != 0) return;
    }
}