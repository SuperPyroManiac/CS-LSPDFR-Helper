using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.SHVDN_Modules;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;

public class SHVDNProcess
{
    internal static async Task ProcessLog(DiscordAttachment attach, MessageCreateEventArgs ctx)
    {
        try
        {
            var log = SHVDNAnalyzer.Run(attach.Url).Result;
            
            DiscordMessageBuilder messageBuilder = new();
            DiscordEmbedBuilder embed = new()
            {
                Description = $"## __ULSS AutoHelper__\r\n**{log.DownloadLink}**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Problems: {log.Scripts.Count}"
                }
            };
            var scriptsList = "\r\n- " + string.Join("\r\n- ", log.Scripts);
            var missingDependsList = "\r\n- " + string.Join("\r\n- ", log.MissingDepends);
            
            if (log.Scripts.Count != 0) 
            {
                embed.AddField(":red_circle:     Some scripts have issues!", "See details below!");
            }
            else 
            {
                embed.AddField(":green_circle:     No faulty script files detected!", "Seems like everything loaded fine.");
            }
            
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Failed Scripts:**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            if (log.Scripts.Count > 0)
                embed2.AddField("Script:", scriptsList, true);
        
            if (log.MissingDepends.Count > 0) 
                embed2.AddField("Missing Depend:", missingDependsList, true);

            messageBuilder.AddEmbed(embed);
            if (log.Scripts.Count > 0) messageBuilder.AddEmbed(embed2);
            if (log.Scripts.Count == 0)
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