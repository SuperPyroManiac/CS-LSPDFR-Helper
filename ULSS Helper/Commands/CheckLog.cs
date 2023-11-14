using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class CheckLog : ApplicationCommandModule
{
    [SlashCommand("CheckLog", "Analyzes the uploaded log. 3MB limit!")]
    public async Task CheckLogCmd(InteractionContext ctx,
        [Option("LogFile", "RagePluginHook.log")]
        DiscordAttachment attach)
    {
        var response = new DiscordInteractionResponseBuilder();
        response.IsEphemeral = true;

        //===//===//===////===//===//===////===//Check Permissions//===////===//===//===////===//===//===//
        
        if (ctx.Member.Roles.All(role => role.Id != 1134534059771572356))
        {
            if (ctx.Channel != ctx.Guild.GetChannel(672541961969729540) && ctx.Channel != ctx.Guild.GetChannel(692254906752696332))
            {
                response.AddEmbed(BasicEmbeds.Error("Invalid channel!\r\nYou may only use this in <#672541961969729540> or <#692254906752696332>!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                return;
            }
            if (string.IsNullOrEmpty(attach.Url))
            {
                response.AddEmbed(BasicEmbeds.Error("There was an error here!\r\nPlease wait a minute and try again!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Failed upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\n\r\nReason denied: Failed to acquire log!"));
                return;
            }
            if (attach.FileName != "RagePluginHook.log")
            {
                response.AddEmbed(BasicEmbeds.Error("Incorrect file name.\r\nPlease make sure your file is called `RagePluginHook.log`!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Warning($"Rejected upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize/1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: Incorrect name"));
                return;
            }
            if (attach.FileSize > 10000000)
            {
                response.AddEmbed(
                    BasicEmbeds.Error(
                        "File is way too big!\r\nYou may not upload anything else until staff review this!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(1134534059771572356));
                Logging.ReportPubLog(BasicEmbeds.Error(
                    $"Possible bot abuse!\r\nUser has been blacklisted from bot use! (Dunce role added!)\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: File way too large! (Larger than 10 MB)"));
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Rejected upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: File way too large! (Larger than 10 MB)"));
                return;
            }
            if (attach.FileSize > 3000000)
            {
                response.AddEmbed(BasicEmbeds.Error("File is too big!\r\nAsk our TS to check this log!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Warning(
                    $"Rejected upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: File too large!"));
                return;
            }
            await ctx.DeferAsync(true);

            //===//===//===////===//===//===////===//Process Attachment//===////===//===//===////===//===//===//
            
            try
            {
                var log = RPHAnalyzer.Run(attach.Url);
                string current;
                List<string?> currentList;
                string outdated;
                string broken;
                string missing;
                string library;
                string missmatch;
                string GTAver = "X";
                string LSPDFRver = "X";
                string RPHver = "X";
                if (Settings.GTAVer.Equals(log.GTAVersion)) GTAver = "\u2713";
                if (Settings.LSPDFRVer.Equals(log.LSPDFRVersion)) LSPDFRver = "\u2713";
                if (Settings.RPHVer.Equals(log.RPHVersion)) RPHver = "\u2713";

                var linkedOutdated = log.Outdated.Select(i => !string.IsNullOrEmpty(i?.Link)
                        ? $"[{i.DName}]({i.Link})"
                        : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})")
                    .ToList();
                currentList = log.Current.Select(i => i?.DName).ToList();
                var brokenList = log.Broken.Select(i => i?.DName).ToList();
                var missingList = log.Missing.Select(i => i?.Name).ToList();
                var missmatchList = log.Missmatch.Select(i => i?.Name).ToList();
                var libraryList = log.Library.Select(i => i?.DName).ToList();
                brokenList.AddRange(libraryList);
                current = string.Join("\r\n- ", currentList);
                outdated = string.Join("\r\n- ", linkedOutdated);
                broken = string.Join("\r\n- ", brokenList);
                missing = string.Join(", ", missingList);
                missmatch = string.Join(", ", missmatchList);
                library = string.Join(", ", libraryList);

                var embed = new DiscordEmbedBuilder
                {
                    Description = "## ULSS Log Reader\r\n*For detailed info, ask for help!*",
                    Color = new DiscordColor(243, 154, 18),
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = SharedLogInfo.TsIcon },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"GTA: {GTAver} - RPH: {RPHver}" +
                               $" - LSPDFR: {LSPDFRver} - Generated in discord.gg/ulss"
                    }
                };

                if (outdated.Length >= 1024 || broken.Length >= 1024)
                {
                    embed.AddField(":warning:     **Message Too Big**",
                        "\r\nToo many plugins to display in a single message.\r\nFor detailed info, ask for help!",
                        true);
                    if (missing.Length > 0) embed.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Title = ":orange_circle:     **Update:**",
                        Description = "\r\n- " + outdated,
                        Color = new DiscordColor(243, 154, 18),
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = SharedLogInfo.TsIcon }
                    };
                    var embed3 = new DiscordEmbedBuilder
                    {
                        Title = ":red_circle:     **Remove:**",
                        Description = "\r\n- " + broken,
                        Color = new DiscordColor(243, 154, 18),
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = SharedLogInfo.TsIcon }
                    };

                    var overflow = new DiscordWebhookBuilder();
                    overflow.AddEmbed(embed);
                    if (outdated.Length != 0) overflow.AddEmbed(embed2);
                    if (broken.Length != 0) overflow.AddEmbed(embed3);
                    await ctx.EditResponseAsync(overflow);

                }
                else
                {
                    if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
                    if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";
                    if ((current.Length == 0 && outdated.Length != 0) || broken.Length != 0) current = "**None**";

                    if (outdated.Length > 0) embed.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
                    if (broken.Length > 0) embed.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);

                    if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0)
                        embed.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
                    if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0 && string.IsNullOrEmpty(log.LSPDFRVersion))
                        embed.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **No plugin information available!**");
                    if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0)
                        embed.AddField(":green_circle:     **No loaded plugins!**", "- No plugins detected from this log.");
                    
                    if (log.Errors.Any(x => x.Level == "CRITICAL"))
                        embed.AddField(":bangbang:     **Critical Error Detected!**", "- You should post this log for our TS to check!");
                
                    DiscordWebhookBuilder webhookBuilder = new();
                    webhookBuilder.AddEmbed(embed);
                    await ctx.EditResponseAsync(webhookBuilder);
                    
                    Logging.SendPubLog(BasicEmbeds.Info($"Successful upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})"));
                    return;
                }
            }
            catch (Exception e)
            {
                response.AddEmbed(BasicEmbeds.Error("There was an error here!\r\nYou may not upload anything else until staff review this!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(1134534059771572356));
                Logging.ReportPubLog(BasicEmbeds.Error(
                    $"Possible bot abuse!\r\nUser has been blacklisted from bot use! (Dunce role added!)\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: Log caused an error! See <#1173304071084585050>"));
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Rejected upload!\r\nSender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\nChannel: <#{ctx.Channel.Id}>\r\nFile name: {attach.FileName}\r\nSize: {attach.FileSize / 1000}KB\r\n[Download Here]({attach.Url})\r\n\r\nReason denied: Log caused an error! See <#1173304071084585050>"));
                Logging.ErrLog($"Public Log Error: {e}");
                Console.WriteLine(e);
                throw;
            }

            //===//===//===////===//===//===////===//Has Dunce Role//===////===//===//===////===//===//===//
            response.AddEmbed(BasicEmbeds.Error(
                "You are blacklisted from the bot!\r\nContact server staff in <#693303741071228938> if you think this is an error!"));
        }
    }
}