using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace ULSS_Helper.Events;

public class VcJoinLeave
{
    public static Task OnMemberJoinLeaveVC(DiscordClient s, VoiceStateUpdateEventArgs ctx)
    {
	    if ((ctx.Channel.UserPermissions & DiscordPermissions.SendMessages) != 0) return Task.CompletedTask;
	    return Task.CompletedTask;
    }
}