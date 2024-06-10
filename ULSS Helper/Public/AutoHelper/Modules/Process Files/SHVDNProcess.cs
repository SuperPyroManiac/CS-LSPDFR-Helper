using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.SHVDN_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;

public class SHVDNProcess
{
    internal static int ProblemCounter(SHVDNLog log)
    {
        if (log == null) return 0;
        else return log.FrozenScripts.Count + log.ScriptDepends.Count;
    }
    internal static async Task ProcessLog(DiscordAttachment attach, MessageCreatedEventArgs ctx)
    {
        try
        {
            var log = await SHVDNAnalyzer.Run(attach.Url);
            
            DiscordMessageBuilder messageBuilder = new();
            DiscordEmbedBuilder embed = new()
            {
                Description = $"## __ULSS AutoHelper__\r\n**{log.DownloadLink}**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Problems: {ProblemCounter(log)}"
                }
            };
            var scriptsList = "\r\n- " + string.Join("\r\n- ", log.FrozenScripts);
            var missingDependsList = "\r\n- " + string.Join("\r\n- ", log.ScriptDepends);
            
            if (log.FrozenScripts.Count != 0) 
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
            if (log.FrozenScripts.Count > 0)
                embed2.AddField("Script:", scriptsList, true);
        
            if (log.ScriptDepends.Count > 0) 
                embed2.AddField("Missing Depend:", missingDependsList, true);

            messageBuilder.AddEmbed(embed);
            if (log.FrozenScripts.Count > 0) messageBuilder.AddEmbed(embed2);
            if (log.FrozenScripts.Count == 0)
                messageBuilder.AddEmbed(BasicEmbeds.Success("__No Issues Detected__\r\n>>> If you do have any problems, you may want to post in the public support channels!", true));
            messageBuilder.AddComponents([
                new DiscordButtonComponent(DiscordButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false,
                    new DiscordComponentEmoji("📨"))]);
            
            await ctx.Message.RespondAsync(messageBuilder);
        }
        catch (Exception e)
        {
            await Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}