using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

public class AutoRPH
{
    internal static async Task<DiscordMessageBuilder> ProccessLog(RPHLog log, MessageCreateEventArgs ctx,
        DiscordThreadChannel st)
    {
        DiscordMessageBuilder messageBuilder = new();
        var gtAver = "❌";
        var lspdfRver = "❌";
        var rpHver = "❌";
        if (Program.Cache.GetPlugin("GrandTheftAuto5").Version.Equals(log.GTAVersion)) gtAver = "\u2713";
        if (Program.Cache.GetPlugin("LSPDFR").Version.Equals(log.LSPDFRVersion)) lspdfRver = "\u2713";
        if (Program.Cache.GetPlugin("RagePluginHook").Version.Equals(log.RPHVersion)) rpHver = "\u2713";
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

        var embdesc = $"## __ULSS Auto Helper - BETA__\r\n**{log.DownloadLink}**";
        if (log.FilePossiblyOutdated)
            embdesc +=
                "\r\n\r\n:warning: **Attention!** This log file is probably too old to determine your current RPH-related issues!";
        var header = BasicEmbeds.Public(embdesc);
        header.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"GTA: {gtAver} - RPH: {rpHver}" + $" - LSPDFR: {lspdfRver} - Generated in Discord.gg/ulss"
        };
        if (outdated.Length != 0 || broken.Length != 0) header.AddField("Plugin Issues Detected!", "> Update or Remove from `GTAV/Plugins/LSPDFR`");
        if (outdated.Length == 0 && broken.Length == 0) header.AddField("Up To Date!", "> All plugins are up to date!");
        
        var embed2 = new DiscordEmbedBuilder
        {
            Title = ":orange_circle:     **Outdated:**",
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
            Title = ":red_circle:     **Broken:**",
            Description = "\r\n>>> " + string.Join(" ᕀ ", brokenList),
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
        };
        embed3.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = "These plugins should be removed! `/CheckPlugin <Name>` for info why!"
        };
        var embed4 = new DiscordEmbedBuilder
        {
            Title = ":bangbang:     **Possible Issues:**",
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
        };

        var update = false;
        foreach (var error in log.Errors)
        {
            if (embed4.Fields.Count == 10)
            {
                embed4.AddField($"___```Oh Merde``` Overflow:___",
                    ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                break;
            }
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
        if (embed4.Fields.Count != 0) messageBuilder.AddEmbed(embed4);
        return messageBuilder;
    }
}