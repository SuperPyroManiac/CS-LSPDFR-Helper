using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.ASI_Modules;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;

public class ASIProcess
{
    internal static async Task ProcessLog(DiscordAttachment attach, MessageCreateEventArgs ctx)
    {
        try
        {
            var log = ASIAnalyzer.Run(attach.Url).Result;
            
            DiscordMessageBuilder messageBuilder = new();
            DiscordEmbedBuilder embed = new()
            {
                Description = $"## __ULSS AutoHelper__\r\n**{log.DownloadLink}**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Loaded ASIs: {log.LoadedAsiFiles.Count} - Failed ASIs: {log.FailedAsiFiles.Count}"
                }
            };
            var failedAsiFilesList = "\r\n- " + string.Join(" ᕀ ", log.FailedAsiFiles);
            
            if (log.FailedAsiFiles.Count == 0)
                embed.AddField(":green_circle:     No faulty ASI files detected!", "Seems like everything loaded fine.");
            if (log.FailedAsiFiles.Count > 0)
                embed.AddField(":red_circle:     Some ASIs failed to load!", "See details below!");
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Failed ASIs:**",
                Description = failedAsiFilesList,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            
            messageBuilder.AddEmbed(embed);
            if (log.FailedAsiFiles.Count > 0) messageBuilder.AddEmbed(embed2);
            if (log.FailedAsiFiles.Count == 0)
                messageBuilder.AddEmbed(BasicEmbeds.Success("__No Issues Detected__\r\n>>> If you do have any problems, please request help so a TS can take a look for you!", true));
            messageBuilder.AddComponents([
                new DiscordButtonComponent(ButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                    new DiscordComponentEmoji("📨"))]);

            await ctx.Message.RespondAsync(messageBuilder);
        }
        catch (Exception e)
        {
            Logging.ErrLog(e.ToString());//TODO: Blacklist
            Console.WriteLine(e);
            throw;
        }
    }
}