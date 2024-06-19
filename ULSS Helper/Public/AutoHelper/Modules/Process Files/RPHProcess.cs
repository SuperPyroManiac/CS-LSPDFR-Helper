using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Public.AutoHelper.Modules.Process_Files;

public class RPHProcess
{
    public static async Task ProcessLog(DiscordAttachment attach, MessageCreatedEventArgs ctx)
    {
        try
        {
            var log = await RPHAnalyzer.Run(attach.Url);
            ProxyCheck.Run(log, Program.Cache.GetUser(ctx.Author.Id.ToString()), ctx.Message);

            if (log.LogModified)
            {
                await ctx.Message.RespondAsync(
                    BasicEmbeds.Error("__AutoBlacklisted!__\r\n" +
                                      "You have sent a modified log! You may not use the AutoHelper until staff review this!", true));
                AutoBlacklist.Add(ctx.Author.Id.ToString(),
                    $">>> User: {ctx.Author.Mention} ({ctx.Author.Id.ToString()})\r\nLog: {ctx.Message.JumpLink}\r\nUser sent a modified log!");
                return;
            }
            
            var linkedOutdated = log.Outdated.Select(
                    i => !string.IsNullOrEmpty(i?.Link)
                        ? $"[{i.DName}]({i.Link})"
                        : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i!.DName.Replace(" ", "+")})").ToList();
            var currentList = log.Current.Select(i => i?.DName).ToList();
            var brokenList = log.Broken.Select(i => i?.DName).ToList();
            brokenList.AddRange(log.Library.Select(i => i?.DName).ToList());
            var current = string.Join("\r\n- ", currentList);
            var outdated = string.Join("\r\n- ", linkedOutdated);
            var broken = string.Join("\r\n- ", brokenList);
            
            var rphProcess = new ULSS_Helper.Modules.RPH_Modules.RPHProcess();
            rphProcess.log = log;
            if (log.Missing.Count > 0 || log.Missmatch.Count > 0)
            {
                await rphProcess.SendUnknownPluginsLog(ctx.Channel.Id, ctx.Author.Id);
            }
            
            var embedDescription = $"## __ULSS AutoHelper__\r\n{log.DownloadLink}";        
            if (log.FilePossiblyOutdated)
                embedDescription += "\r\n:warning: **Attention!** This log file is probably too old to determine the current RPH-related issues of the uploader!\r\n";

            var embed = rphProcess.GetBaseLogInfoEmbed(embedDescription);
            
            if (outdated.Length >= 1024 || broken.Length >= 1024)
            {
                embed.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nResponse will be sent in multiple messages.", true);
                var embed2 = new DiscordEmbedBuilder
                {
                    Title = ":orange_circle:     **Update:**",
                    Description = "\r\n>>> " + string.Join(" - ", linkedOutdated),
                    Color = new DiscordColor(243, 154, 18),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
                };
                var embed3 = new DiscordEmbedBuilder
                {
                    Title = ":red_circle:     **Remove:**",
                    Description = "\r\n>>> " + string.Join(" - ", brokenList),
                    Color = new DiscordColor(243, 154, 18),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
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

                var overflowBuilder = new DiscordMessageBuilder();
                overflowBuilder.AddEmbed(embed);
                if (outdated.Length != 0) overflowBuilder.AddEmbed(embed2);
                if (broken.Length != 0) overflowBuilder.AddEmbed(embed3);
                if (embed4.Fields.Count != 0) overflowBuilder.AddEmbed(embed4);
                // ReSharper disable RedundantExplicitParamsArrayCreation
                overflowBuilder.AddComponents([
                    new DiscordButtonComponent(DiscordButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false, new DiscordComponentEmoji("📨"))]);
                
                await ctx.Message.RespondAsync(overflowBuilder);
                 
            }
            else
            {
                if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
                if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";

                if (outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n>>> - " + outdated, true);
                if (broken.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n>>> - " + broken, true);

                if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && !string.IsNullOrEmpty(log.LSPDFRVersion))
                    embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
                if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
                    embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
                if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
                    embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
        
                if (current.Length == 0) current = "**None**";
                
                var update = false;
                var ecnt = 0;
                foreach (var error in log.Errors)
                {
                    if (embed.Fields.Count == 20)
                    {
                        embed.AddField($"___```Too Much``` Overflow:___",
                            ">>> You have more errors than we can show!\r\nPlease fix what is shown, and upload a new log!");
                        break;
                    }
                    if (error.Level == "AUTO") continue;
                    if (error.Level == "CRITICAL") update = true;
                    if (update)
                        if (error.Level == "CRITICAL")
                            embed.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                                $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
                    if (!update)
                        if (error.Level != "XTRA")
                        {
                            embed.AddField($"___```{error.Level} ID: {error.ID}``` Troubleshooting Steps:___",
                                $"> {error.Solution.Replace("\n", "\n> ")}\r\n> ___*Generated in discord.gg/ulss*___");
                            ecnt++;
                        }
                }
                
                DiscordMessageBuilder webhookBuilder = new();
                if (outdated.Length == 0 && broken.Length == 0 && ecnt == 0 && !update)
                    webhookBuilder.AddEmbed(BasicEmbeds.Success("__No Issues Detected__\r\n>>> If you do have any problems, you may want to post in the public support channels!", true));
                else
                {
                    webhookBuilder.AddEmbed(embed);
                }
                webhookBuilder.AddComponents(
                    [
                        new DiscordButtonComponent(DiscordButtonStyle.Secondary, ComponentInteraction.SendFeedback, "Send Feedback", false, new DiscordComponentEmoji("📨"))
                    ]
                );
                await ctx.Message.RespondAsync(webhookBuilder);
            }
        }
        catch (Exception e)
        {
            await Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
        }
    }
}