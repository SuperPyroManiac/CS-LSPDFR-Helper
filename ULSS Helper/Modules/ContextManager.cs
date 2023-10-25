using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules;

internal class ContextManager : ApplicationCommandModule
{
    private static string current;
    private static List<string?> currentList;
    private static string outdated;
    private static string broken;
    private static string missing;
    private static string library;
    private static AnalyzedLog log;
    private static string? _file;
    private static string GTAver = "X";
    private static string LSPDFRver = "X";
    private static string RPHver = "X";
    private const string TsRoleGearsIconUrl = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless";
    private const string LogUploaderFieldName = "Log uploader:";
    private const string PluginsNotRecognizedFieldName = ":bangbang:  **Plugins not recognized:**";
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    public static async Task OnMenuSelect(ContextMenuContext e)
    {
        if (e.Member.Roles.All(role => role.Id != Settings.GetTSRole()))//TODO: Proper permissions setup
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(MessageManager.Error("You do not have permission for this!"));
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }
        try
        {
            switch (e.TargetMessage.Attachments.Count)
            {
                case 0:
                    var emb = new DiscordInteractionResponseBuilder();
                    emb.IsEphemeral = true;
                    emb.AddEmbed(MessageManager.Error("No attachment found. There needs to be a file named `RagePluginHook.log` attached to the message!"));
                    await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                    return;
                case 1:
                    _file = e.TargetMessage.Attachments[0]?.Url;
                    break;
                case > 1:
                    foreach(DiscordAttachment attachment in e.TargetMessage.Attachments)
                    {
                        if (attachment.Url.Contains("RagePluginHook.log"))
                        {
                            _file = attachment.Url;
                            break;
                        }

                    }
                    if (_file == null)
                    {
                        var emb2 = new DiscordInteractionResponseBuilder();
                        emb2.IsEphemeral = true;
                        emb2.AddEmbed(MessageManager.Error("There is no file named `RagePluginHook.log!`"));
                        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb2);
                        return;
                    }
                    break;
            }
            if (_file == null)
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(MessageManager.Error("Failed to load `RagePluginHook.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            if (!_file.Contains("RagePluginHook"))
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(MessageManager.Error("This file is not named `RagePluginHook.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }

            await e.DeferAsync(true);

            //========================================================================================================
            
            using var client = new WebClient();
            client.DownloadFile(_file,
                Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", Settings.LogNamer())); //Download the file
            log = LogAnalyzer.Run(); //Process the log
            
            var linkedOutdated = new List<string>();
            foreach (var i in log.Outdated)
            {
                if (i.Link != null)
                {
                    linkedOutdated.Add($"[{i.DName}]({i.Link})");
                }
                else
                {
                    linkedOutdated.Add($"[{i.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})");
                }
            }

            currentList = log.Current.Select(i => i?.DName).ToList();
            var brokenList = log.Broken.Select(i => i?.DName).ToList();
            var missingList = log.Missing.Select(i => i?.Name).ToList();
            var libraryList = log.Library.Select(i => i?.DName).ToList();
            brokenList.AddRange(libraryList);
            current = string.Join("\r\n- ", currentList);
            outdated = string.Join("\r\n- ", linkedOutdated);
            broken = string.Join("\r\n- ", brokenList);
            missing = string.Join(", ", missingList);
            library = string.Join(", ", libraryList);

            DiscordEmbedBuilder message = GetBaseLogInfoMessage("# **Quick Log Information**");

            message.AddField(LogUploaderFieldName, $"<@{e.TargetMessage.Author.Id}>", true); // don't change the field value here without changing the GetLogOwnerId method.
            message.AddField("Log message:", e.TargetMessage.JumpLink.ToString(), true);
            message.AddField("\u200B", "\u200B", true); // This invisible field (zero-width characters) is necessary in order to force the next inline field to be in a new row. 
            
            if (outdated.Length >= 1024 || broken.Length >= 1024)
            {
                message.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.", true);
                if (missing.Length > 0) message.AddField(PluginsNotRecognizedFieldName, missing, false);
                var message2 = new DiscordEmbedBuilder { Title = ":orange_circle:     **Update:**", Description = "\r\n- " + outdated, Color = DiscordColor.Gold };
                message2.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl };
                var message3 = new DiscordEmbedBuilder { Title = ":red_circle:     **Remove:**", Description = "\r\n- " + broken, Color = DiscordColor.Gold };
                message3.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl };
                
                var overflow = new DiscordWebhookBuilder();
                overflow.AddEmbed(message);
                if (outdated.Length != 0) overflow.AddEmbed(message2);
                if (broken.Length != 0) overflow.AddEmbed(message3);
                overflow.AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info", false,
                        new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false,
                        new DiscordComponentEmoji("📨"))
                });
                await e.EditResponseAsync(overflow);
            }
            else
            {
                message = AddCommonFields(message);
                
                await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info (WIP)", false, new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false, new DiscordComponentEmoji("📨"))
                }));
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
    
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        // names of embed fields that should only be shown to ULSS staff
        List<string> tsInfoFieldNames = new List<string>{
            "Log message:",
            LogUploaderFieldName,
            "\u200B"
        };
        if (e.Id == "send" || e.Id == "send2") await SendMessageToUser(e, tsInfoFieldNames);
        if (e.Id == "info") await SendDetailedInfoMessage(e, tsInfoFieldNames);
    }

    internal static async Task SendMessageToUser(ComponentInteractionCreateEventArgs e, List<string> tsInfoFieldNames)
    {
        await e.Interaction.DeferAsync(true);
        
        ulong? logOwnerId = null;
        List<DiscordEmbed> userMessageEmbeds = new List<DiscordEmbed>();
        tsInfoFieldNames.Add(PluginsNotRecognizedFieldName);
        
        foreach(DiscordEmbed originalEmbed in e.Message.Embeds) 
        {
            // Filters fields from DiscordEmbeds that should not be visible for everyone once the message is being sent to the user.
            if (originalEmbed.Fields != null && originalEmbed.Fields.Any(field => tsInfoFieldNames.Contains(field.Name)))
            {
                var newEmbed = new DiscordEmbedBuilder();
                newEmbed.Color = originalEmbed.Color;
                if (originalEmbed.Description != null)
                    newEmbed.Description = originalEmbed.Description;
                if (originalEmbed.Author != null)
                    newEmbed.Author = new DiscordEmbedBuilder.EmbedAuthor() 
                    { 
                        Name = originalEmbed.Author.Name, 
                        IconUrl = originalEmbed.Author.IconUrl.ToString() 
                    };
                if (originalEmbed.Thumbnail != null)
                    newEmbed.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = originalEmbed.Thumbnail.Url.ToString() };
                if (originalEmbed.Footer != null)
                    newEmbed.Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = originalEmbed.Footer.Text };

                foreach (DiscordEmbedField originalField in originalEmbed.Fields) 
                {
                    if (!tsInfoFieldNames.Contains(originalField.Name))
                    {
                        newEmbed.AddField(originalField.Name, originalField.Value, originalField.Inline);
                    }
                    if (logOwnerId == null && originalField.Name.Equals(LogUploaderFieldName)) 
                    {
                        logOwnerId = GetLogOwnerId(originalField.Value);
                    }
                }
                userMessageEmbeds.Add(newEmbed);
            } 
            else 
            {
                userMessageEmbeds.Add(originalEmbed);
            }
        }

        var newMessage = new DiscordMessageBuilder();
        newMessage.AddEmbeds(userMessageEmbeds);
        if (logOwnerId != null) 
        {
            UserMention logOwner = new UserMention((ulong) logOwnerId);
            newMessage.AddMention(logOwner);
            newMessage.WithAllowedMention(logOwner);
            newMessage.WithContent($"<@{logOwnerId}>");
        }
        await e.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(e.Channel);
    }

    internal static async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs e, List<string> tsInfoFieldNames)
    {
        ulong? logOwnerId = null;
        await e.Interaction.DeferAsync(true);
        
        DiscordEmbedBuilder message = GetBaseLogInfoMessage("# **Detailed Log Information**");

        foreach(DiscordEmbed originalEmbed in e.Message.Embeds) 
        {
            foreach (DiscordEmbedField originalField in e.Message.Embeds[0].Fields) 
            {
                if (
                    message.Fields != null
                    && !message.Fields.Any(msgField => msgField.Name.Equals(originalField.Name)) 
                    && tsInfoFieldNames.Contains(originalField.Name)
                )
                {
                    message.AddField(originalField.Name, originalField.Value, originalField.Inline);
                }
                if (logOwnerId == null && originalField.Name.Equals(LogUploaderFieldName)) 
                {
                    logOwnerId = GetLogOwnerId(originalField.Value);
                }
            }
        }
        
        if (outdated.Length >= 1024 || broken.Length >= 1024 || current.Length >= 1024)
        {
            message.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor error checking please first fix plugin issues.", true);
            if (missing.Length > 0) message.AddField(PluginsNotRecognizedFieldName, missing, false);
            var currentMsg = new DiscordEmbedBuilder
            {
                Title = ":green_circle:     **Current:**",
                Description = "\r\n- " + current,
                Color = DiscordColor.Gold,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl }
            };
            var outdatedMsg = new DiscordEmbedBuilder
            {
                Title = ":orange_circle:     **Update:**",
                Description = "\r\n- " + outdated,
                Color = DiscordColor.Gold,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl }
            };
            var brokenMsg = new DiscordEmbedBuilder
            {
                Title = ":red_circle:     **Remove:**",
                Description = "\r\n- " + broken,
                Color = DiscordColor.Gold,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl }
            };
            
            var overflow = new DiscordWebhookBuilder();
            overflow.AddEmbed(message);
            // if (current.Length != 0) overflow.AddEmbed(currentPluginsMsg);
            if (outdated.Length != 0) overflow.AddEmbed(outdatedMsg);
            if (broken.Length != 0) overflow.AddEmbed(brokenMsg);
            overflow.AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "send2", "Send To User", false,
                    new DiscordComponentEmoji("📨"))
            });
            await e.Interaction.EditOriginalResponseAsync(overflow);
        }
        else
        {
            message = AddCommonFields(message);

            foreach (var error in log.Errors)
            {
                message.AddField($"```ID: {error.ID}``` {error.Level} Error Info", $"> {error.Solution}");
            }
            
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Danger, "send2", "Send To User", false, new DiscordComponentEmoji("📨"))
            }));
        }
    }

    private static DiscordEmbedBuilder GetBaseLogInfoMessage(string description)
    {
        if (Settings.GTAVer == log.GTAVersion) GTAver = "\u2713";
        if (Settings.LSPDFRVer == log.LSPDFRVersion) LSPDFRver = "\u2713";
        if (Settings.RPHVer == log.RPHVersion) RPHver = "\u2713";

        return new DiscordEmbedBuilder(){
            Description = description,
            Color = DiscordColor.Gold,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsRoleGearsIconUrl },
            Footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = $"GTA: {GTAver} - RPH: {RPHver} - LSPDFR: {LSPDFRver} - Errors: {log.Errors.Count}"
            }
        };
            
    }

    private static DiscordEmbedBuilder AddCommonFields(DiscordEmbedBuilder message)
    {
        if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
        if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";
        if (current.Length == 0 && outdated.Length != 0 || broken.Length != 0) current = "**None**";

        //if (current.Length > 0) message.AddField(":green_circle:     **Current:**", "\r\n" + string.Join(", ", currentList), false);
        if (outdated.Length > 0) message.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
        if (broken.Length > 0) message.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);
        if (missing.Length > 0) message.AddField(PluginsNotRecognizedFieldName, missing, false);
    
        if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0) message.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
        if (LSPDFRver == "X") message.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **You should manually check the log!**");
        if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0 && LSPDFRver != "X") message.AddField(":green_circle:     **No installed plugins!**", "- Can't have plugin issues if you don't got any!");

        return message;
    }

    private static ulong? GetLogOwnerId(string logUploaderMention) 
    {
        try
        { 
            string logOwnerIdFromTag = logUploaderMention.Split("<@")[1].Split(">")[0];
            return ulong.Parse(logOwnerIdFromTag);
        }
        catch (Exception ex)            
        {                
            if (ex is ArgumentNullException || ex is FormatException || ex is OverflowException)
            {
                Console.WriteLine("Couldn't get logOwnerId. Field value is:", logUploaderMention);
                return null;
            }
            throw;
        }
    }
}