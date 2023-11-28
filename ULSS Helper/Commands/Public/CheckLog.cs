using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Commands.Public;

public class CheckLog : ApplicationCommandModule
{
    [SlashCommand("CheckLog", "Analyzes the uploaded log. 3MB limit!")]
    public async Task CheckLogCmd(InteractionContext ctx,
        [Option("LogFile", "RagePluginHook.log")]
        DiscordAttachment attachment)
    {
        var response = new DiscordInteractionResponseBuilder();
        response.IsEphemeral = true;

        //===//===//===////===//===//===////===//Check Permissions//===////===//===//===////===//===//===//
        
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.BotBlacklistRoleId))
        {
            List<ulong> allowedChannelIds = Program.Settings.Env.PublicUsageAllowedChannelIds;
            if (allowedChannelIds.All(allowedId => ctx.Channel != ctx.Guild.GetChannel(allowedId)))
            {
                List<string> allowedChannels = allowedChannelIds.Select(selector: channelId => $"<#{channelId}>").ToList();
                response.AddEmbed(BasicEmbeds.Error($"Invalid channel!\r\nYou may only use this in {string.Join(" or ", allowedChannels)}!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                return;
            }
            if (string.IsNullOrEmpty(attachment.Url))
            {
                response.AddEmbed(BasicEmbeds.Error("There was an error here!\r\nPlease wait a minute and try again!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Failed upload!\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n\r\n"
                    + $"Reason denied: Failed to acquire log!"
                ));
                return;
            }
            if (attachment.FileName != "RagePluginHook.log")
            {
                response.AddEmbed(BasicEmbeds.Error("Incorrect file name.\r\nPlease make sure your file is called `RagePluginHook.log`!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Warning(
                    $"Rejected upload!\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize/1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: Incorrect name"
                ));
                return;
            }
            if (attachment.FileSize > 10000000)
            {
                response.AddEmbed(BasicEmbeds.Error("File is way too big!\r\nYou may not upload anything else until staff review this!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(Program.Settings.Env.BotBlacklistRoleId));
                Logging.ReportPubLog(BasicEmbeds.Error(
                    $"Possible bot abuse!\r\n"
                    + $"User has been blacklisted from bot use! (Dunce role added!)\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: File way too large! (Larger than 10 MB)"
                ));
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Rejected upload!\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: File way too large! (Larger than 10 MB)"
                ));
                return;
            }
            if (attachment.FileSize > 3000000)
            {
                response.AddEmbed(BasicEmbeds.Error("File is too big!\r\nAsk our TS to check this log!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                Logging.SendPubLog(BasicEmbeds.Warning(
                    $"Rejected upload!\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: File too large!"
                ));
                return;
            }
            await ctx.DeferAsync(true);

            //===//===//===////===//===//===////===//Process Attachment//===////===//===//===////===//===//===//
            try
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
	            var th = new Thread(() => CheckLogMessage(ctx, attachment));
	            th.Start();
            }
            catch (Exception e)
            {
                response.AddEmbed(BasicEmbeds.Error("There was an error here!\r\nYou may not upload anything else until staff review this!"));
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
                await ctx.Member.GrantRoleAsync(ctx.Guild.GetRole(Program.Settings.Env.BotBlacklistRoleId));
                Logging.ReportPubLog(BasicEmbeds.Error(
                    $"Possible bot abuse!\r\n"
                    + $"User has been blacklisted from bot use! (Dunce role added!)\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: Log caused an error! See <#{Program.Settings.Env.TsBotLogChannelId}>"
                ));
                Logging.SendPubLog(BasicEmbeds.Error(
                    $"Rejected upload!\r\n"
                    + $"Sender: <@{ctx.Member.Id}> ({ctx.Member.Username})\r\n"
                    + $"Channel: <#{ctx.Channel.Id}>\r\n"
                    + $"File name: {attachment.FileName}\r\n"
                    + $"Size: {attachment.FileSize / 1000}KB\r\n"
                    + $"[Download Here]({attachment.Url})\r\n\r\n"
                    + $"Reason denied: Log caused an error! See <#{Program.Settings.Env.TsBotLogChannelId}>"
                ));
                Logging.ErrLog($"Public Log Error: {e}");
                Console.WriteLine(e);
                throw;
            }

            //===//===//===////===//===//===////===//Has Dunce Role//===////===//===//===////===//===//===//
            response.AddEmbed(BasicEmbeds.Error(
                $"You are blacklisted from the bot!\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!"));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }

    private async Task CheckLogMessage(InteractionContext context, DiscordAttachment attach)
    {
        var log = RPHAnalyzer.Run(attach.Url);
        string gtAver = "X";
        string lspdfRver = "X";
        string rpHver = "X";
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
	        // ReSharper disable once UseObjectOrCollectionInitializer
	        RPHProcess rphProcess = new RPHProcess();
            rphProcess.log = log;
            rphProcess.SendUnknownPluginsLog(context.Channel.Id, context.Member.Id);
        }

        string embedDescription = "## ULSS Log Reader\r\n*For detailed info, ask for help!*";
        if (log.FilePossiblyOutdated)
            embedDescription += "\r\n\r\n:warning: **Attention!** This log file is probably too old to determine your current RPH-related issues!";
        if (outdated.Length > 0 || broken.Length > 0) 
            embedDescription += "\r\n\r\nUpdate or remove the following files in `GTAV/plugins/LSPDFR`";

        var embed = new DiscordEmbedBuilder
        {
            Description = embedDescription,
            Color = new DiscordColor(243, 154, 18),
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"GTA: {gtAver} - RPH: {rpHver}" +
                       $" - LSPDFR: {lspdfRver} - Generated in discord.gg/ulss"
            }
        };

        if (outdated.Length >= 1024 || broken.Length >= 1024)
        {
            embed.AddField(":warning:     **Message Too Big**",
                "\r\nToo many plugins to display in a single message.\r\nFor detailed info, ask for help!",
                true);
            var embed2 = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n- " + outdated,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };
            var embed3 = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n- " + broken,
                Color = new DiscordColor(243, 154, 18),
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = Program.Settings.Env.TsIconUrl }
            };

            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(embed);
            if (outdated.Length != 0) overflow.AddEmbed(embed2);
            if (broken.Length != 0) overflow.AddEmbed(embed3);
            await context.EditResponseAsync(overflow);

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
                    
            if (log.Errors.Any(error => error.Level == "CRITICAL" || error.Level == "SEVERE"))
                embed.AddField(":bangbang:     **Serious Error Detected!**", "- You should post this log for our TS to check!");
                
            DiscordWebhookBuilder webhookBuilder = new();
            webhookBuilder.AddEmbed(embed);
            // ReSharper disable once RedundantExplicitParamsArrayCreation
            webhookBuilder.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Secondary, "SendFeedback", "Send Feedback", false,
                    new DiscordComponentEmoji("📨"))
            });
            await context.EditResponseAsync(webhookBuilder);
                    
            Logging.SendPubLog(BasicEmbeds.Info(
                $"Successful upload!\r\n"
                + $"Sender: <@{context.Member.Id}> ({context.Member.Username})\r\n"
                + $"Channel: <#{context.Channel.Id}>\r\n"
                + $"File name: {attach.FileName}\r\n"
                + $"Size: {attach.FileSize / 1000}KB\r\n"
                + $"[Download Here]({attach.Url})"
            ));
        }
    }
}