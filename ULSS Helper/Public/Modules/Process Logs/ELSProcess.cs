using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.ELS_Modules;

namespace ULSS_Helper.Public.Modules.Process_Logs;

public class ELSProcess
{
    internal static async Task ProcessLog(DiscordAttachment attach, MessageCreateEventArgs ctx)
    {
        try
        {
            var log = ELSAnalyzer.Run(attach.Url).Result;

            DiscordMessageBuilder messageBuilder = new();
            DiscordEmbedBuilder embed = new()
            {
                Description = $"## __ULSS AutoHelper__\r\n**{log.DownloadLink}**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"ELS Version: {log.ElsVersion} - AdvancedHookV installed: {(log.AdvancedHookVFound ? "\u2713" : "X")}"
                }
            };
            DiscordEmbedBuilder embed2 = new();
            var invalidVcFiles = "\r\n- " + string.Join(" ᕀ ", log.InvalidElsVcfFiles);
            
            if (log.FaultyVcfFile != null) 
                embed.AddField(":red_circle:     Faulty VCF found!", $"Remove `{log.FaultyVcfFile}` from `{log.VcfContainer}`");

            if (log.InvalidElsVcfFiles.Count > 0)
            {
                embed2 = new DiscordEmbedBuilder
                {
                    Title = ":orange_circle:     **Invalid VCFs:**",
                    Description = invalidVcFiles,
                    Color = new DiscordColor(243, 154, 18),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
                };
            }

            if (log.FaultyVcfFile == null)
                embed.AddField(":green_circle:     No faulty VCF files detected!", "Seems like ELS loaded fine. If ELS still is not working correctly, make sure to check the asiloader.log as well!");

            messageBuilder.AddEmbed(embed);
            if (log.InvalidElsVcfFiles.Count > 0) messageBuilder.AddEmbed(embed2);
            if (log.InvalidElsVcfFiles.Count == 0 && log.FaultyVcfFile == null)
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