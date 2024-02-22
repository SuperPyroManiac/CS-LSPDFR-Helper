using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.ComponentModel.Design;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;

public class RPHProcess
{
    internal static async Task ProcessLog(DiscordAttachment attach, MessageCreateEventArgs ctx)
    {
        try
        {
            var log = RPHAnalyzer.Run(attach.Url).Result;
            DiscordMessageBuilder messageBuilder = new();
            var gtAver = "❌";
            var lspdfRver = "❌";
            var rpHver = "❌";
            if (Program.Cache.GetPlugin("GrandTheftAuto5").Version.Equals(log.GTAVersion)) gtAver = "\u2713";
            if (Program.Cache.GetPlugin("LSPDFR").Version.Equals(log.LSPDFRVersion)) lspdfRver = "\u2713";
            if (Program.Cache.GetPlugin("RagePluginHook").Version.Equals(log.RPHVersion)) rpHver = "\u2713";
            var linkedOutdated = log.Outdated.Select(
                    i => !string.IsNullOrEmpty(i?.Link)
                        ? $"[{i.DName}]({i.Link})"
                        : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i!.DName.Replace(" ", "+")})")
                .ToList();
            var currentList = log.Current.Select(i => i?.DName).ToList();
            var brokenList = log.Broken.Select(i => i?.DName).ToList();
            var causedCrashList = log.CausedCrash.Select(i => i?.DName).ToList();
            brokenList.AddRange(log.Library.Select(i => i?.DName).ToList());
            var current = string.Join("\r\n- ", currentList);
            var outdated = string.Join("\r\n- ", linkedOutdated);
            var broken = string.Join("\r\n- ", brokenList);
            var causedCrash = string.Join("\r\n- ", causedCrashList);

            if (log.Missing.Count > 0 || log.Missmatch.Count > 0)
            {
                var rphProcess = new ULSS_Helper.Modules.RPH_Modules.RPHProcess();
                rphProcess.log = log;
                rphProcess.SendUnknownPluginsLog(ctx.Channel.Id, ctx.Author.Id);
            }

            var embdesc = $"## __ULSS AutoHelper__\r\n**{log.DownloadLink}**";
            if (log.FilePossiblyOutdated)
                embdesc +=
                    "\r\n\r\n:warning: **Attention!** This log file is probably too old to determine your current RPH-related issues!";
            if (log.MultipleSessions)
                embdesc +=
                    "\r\n\r\n:warning: **Attention!** This log file contains multiple LSPDFR sessions!\nThe plugin checks will only be done to your first LSPDFR session and therefore might be incorrect, if you changed anything in your `GTAV/plugins/LSPDFR` folder";
            var header = BasicEmbeds.Public(embdesc);
            header.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"GTA: {gtAver} - RPH: {rpHver}" + $" - LSPDFR: {lspdfRver} - Generated in Discord.gg/ulss"
            };
            if (outdated.Length != 0 || broken.Length != 0 || causedCrash.Length != 0) header.AddField("Plugin Issues Detected!", "> Update or Remove from `GTAV/Plugins/LSPDFR`");
            if (outdated.Length == 0 && broken.Length == 0 && causedCrash.Length == 0) header.AddField("Up To Date!", "> All plugins are up to date!");

            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update Required:**",
                Description = "\r\n>>> " + string.Join(" ᕀ ", linkedOutdated),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            embed2.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "These plugins need to be updated!"
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Known To Cause Problems:**",
                Description = "\r\n>>> " + string.Join(" ᕀ ", brokenList),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            embed3.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "We recommend removing these, /CheckPlugin for reason why!"
            };
            var embed5 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Caused LSPDFR to crash:**",
                Description = "\r\n>>> " + string.Join(" ᕀ ", causedCrashList),
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            if (causedCrashList.Count > 1)
            {
                embed5.Footer = new DiscordEmbedBuilder.EmbedFooter
                { Text = "Remove these plugins and inform the developers!" };
            }
            else
            {
                embed5.Footer = new DiscordEmbedBuilder.EmbedFooter
                { Text = "Remove this plugin and inform the developer!" };
            }
            var embed4 = new DiscordEmbedBuilder
            {
                Title = ":bangbang:     **Possible Issues:**",
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var update = false;
            foreach (var error in log.Errors)
            {
                if (embed4.Fields.Count == 20)
                {
                    embed4.AddField($"___```Too Much``` Overflow:___",
                        ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                    break;
                }
                if (error.Level == "AUTO") continue;
                if (error.Level == "CRITICAL") update = true;
                if (update)
                    if (error.Level == "CRITICAL")
                        embed4.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                            $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
                if (!update)
                    if (error.Level != "XTRA")
                        embed4.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                            $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
            }

            messageBuilder.AddEmbed(header);
            if (outdated.Length != 0) messageBuilder.AddEmbed(embed2);
            if (broken.Length != 0) messageBuilder.AddEmbed(embed3);
            if (causedCrash.Length != 0) messageBuilder.AddEmbed(embed5);
            if (embed4.Fields.Count != 0) messageBuilder.AddEmbed(embed4);
            if (outdated.Length == 0 && broken.Length == 0 && embed4.Fields.Count == 0 && causedCrash.Length == 0)
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