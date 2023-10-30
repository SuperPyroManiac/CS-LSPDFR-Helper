using DSharpPlus;
using DSharpPlus.Entities;

namespace ULSS_Helper.Modules.Messages;

internal class ErrorHandler
{
    internal static DiscordInteractionResponseBuilder ErrEmb()
    {
        var err = new DiscordInteractionResponseBuilder();
        err.IsEphemeral = true;
        err.AddEmbed(BasicEmbeds.Error("The bot encountered a serious error! See <#1168438186939273276> for details!"));
        return err;
    }

    internal static void ErrLog(string e)
    {
        var log = new DiscordMessageBuilder()
            .WithContent(e)
            .SendAsync(Program.Client.GetChannelAsync(1168438186939273276).Result);
    }
}