using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

public class AutoRPH
{
    internal static async Task ProccessLog(RPHLog log, MessageCreateEventArgs ctx, DiscordThreadChannel st)
    {
        var gtAver = "X";
        var lspdfRver = "X";
        var rpHver = "X";
        if (Program.Settings.Env.GtaVersion.Equals(log.GTAVersion)) gtAver = "\u2713";
        if (Program.Settings.Env.LspdfrVersion.Equals(log.LSPDFRVersion)) lspdfRver = "\u2713";
        if (Program.Settings.Env.RphVersion.Equals(log.RPHVersion)) rpHver = "\u2713";
        var linkedOutdated = log.Outdated.Select(i => !string.IsNullOrEmpty(i?.Link)
                ? $"[{i.DName}]({i.Link})"
                : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i!.DName.Replace(" ", "+")})")
            .ToList();
        var currentList = log.Current.Select(i => i?.DName).ToList();
        var brokenList = log.Broken.Select(i => i?.DName).ToList();
        var libraryList = log.Library.Select(i => i?.DName).ToList();
        brokenList.AddRange(libraryList);
        var current = string.Join("\r\n- ", currentList);
        var outdated = string.Join("\r\n- ", linkedOutdated);
        var broken = string.Join("\r\n- ", brokenList);
        
        if (log.Missing.Count > 0 || log.Missmatch.Count > 0) 
        {
            var rphProcess = new RPHProcess();
            rphProcess.log = log;
            rphProcess.SendUnknownPluginsLog(ctx.Channel.Id, ctx.Author.Id);
        }
        
        var embdesc = "## ULSS Auto Helper - BETA\r\n*Plugin Information*";
        if (log.FilePossiblyOutdated)
            embdesc += "\r\n\r\n:warning: **Attention!** This log file is probably too old to determine your current RPH-related issues!";
        if (outdated.Length > 0 || broken.Length > 0) 
            embdesc += "\r\n\r\nUpdate or remove the following files in `GTAV/plugins/LSPDFR`";
        
        var header = BasicEmbeds.Public(embdesc);
        var fs = new FileStream(Path.Combine(log.FilePath), FileMode.Open, FileAccess.Read);

        header.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"GTA: {gtAver} - RPH: {rpHver}" + $" - LSPDFR: {lspdfRver} - Generated in Discord.gg/ulss"
        };
        
                if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            header.AddField(":warning:     **Message Too Big**",
                "\r\nToo many plugins to display in a single message.\r\nFor detailed info, ask for help!",
                true);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n>>> - " + outdated,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n>>> - " + broken,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflow = new DiscordMessageBuilder();
            overflow.AddEmbed(header);
            overflow.AddFile(fs, AddFileOptions.CloseStream);
            if (outdated.Length != 0) overflow.AddEmbed(embed2);
            if (broken.Length != 0) overflow.AddEmbed(embed3);
            await st.SendMessageAsync(overflow);

        }
        else
        {
            if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
            if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";
            if ((current.Length == 0 && outdated.Length != 0) || broken.Length != 0) current = "**None**";

            if (outdated.Length > 0) header.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + outdated, true);
            if (broken.Length > 0) header.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + broken, true);

            if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0)
                header.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
            if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
                header.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
            if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
                header.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
                    
            if (log.Errors.Any(error => error.Level == "CRITICAL" || error.Level == "SEVERE"))
                header.AddField(":bangbang:     **Serious Error(s) Detected!**", "You should post this log for our TS to check! The bot may not always be 100% correct!");

            var update = false;
            foreach (var error in log.Errors)
            {
                if (error.Level == "CRITICAL") update = true;
                if (update)
                {
                    if (error.Level == "CRITICAL")
                        header.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
                }
                if (!update)
                {
                    if (error.Level != "XTRA")
                        header.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                        $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
                }
            }
            header.AddField($"___```NOTICE``` Bot Rules:___",
                ">>> __**This is for ULSS use only!**__\r\n"
                + "- Do not use this for proxy support!\r\n"
                + "- Do not repost this for someone else!\r\n"
                + "- Do not claim this as your own support!\r\n"
                + "- Do not use this for support on LCPDFR.com!\r\n"
                + "*Failure to comply will result in access being revoked!*");
                
            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(header);
            messageBuilder.AddFile(fs, AddFileOptions.CloseStream);
            // ReSharper disable once RedundantExplicitParamsArrayCreation
            messageBuilder.AddComponents([
                new DiscordButtonComponent(ButtonStyle.Secondary, "SendFeedback", "Send Feedback", false,
                    new DiscordComponentEmoji("📨"))
            ]);
            await st.SendMessageAsync(messageBuilder);
        }
                
        Thread.Sleep(30000);
        await st.DeleteAsync();
        await ctx.Message.DeleteAsync();
    }
}