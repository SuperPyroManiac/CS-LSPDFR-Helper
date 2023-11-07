using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Modules.LogAnalyzer;
using ULSS_Helper.Modules.Messages;

namespace ULSS_Helper.Modules;

internal class ContextManager : ApplicationCommandModule
{
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]

    public static async Task OnMenuSelect(ContextMenuContext e)
    {
        if (e.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(BasicEmbeds.Error("You do not have permission for this!"));
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }

        try
        {
            await LogAnalyzerManager.ProcessAttachments(e);
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
}