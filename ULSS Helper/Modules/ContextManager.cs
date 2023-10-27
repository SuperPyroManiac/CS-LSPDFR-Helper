using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using static ULSS_Helper.Modules.MessageManager;

namespace ULSS_Helper.Modules;

internal class ContextManager : ApplicationCommandModule
{
    internal static string current;
    private static List<string?> currentList;
    internal static string outdated;
    internal static string broken;
    internal static string missing;
    internal static string library;
    internal static string missmatch;
    private static ulong senderID;
    internal static AnalyzedLog log;
    private static string? _file;
    internal static string GTAver = "X";
    internal static string LSPDFRver = "X";
    internal static string RPHver = "X";
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    public static async Task OnMenuSelect(ContextMenuContext e)
    {
        if (e.Member.Roles.All(role => role.Id != Settings.GetTSRole()))//TODO: Proper permissions setup
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(Error("You do not have permission for this!"));
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }
        
        //===============================================CHECK ATTACHMENTS=========================================================
        
        try
        {
            switch (e.TargetMessage.Attachments.Count)
            {
                case 0:
                    var emb = new DiscordInteractionResponseBuilder();
                    emb.IsEphemeral = true;
                    emb.AddEmbed(Error("No attachment found. There needs to be a file named `RagePluginHook.log` attached to the message!"));
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
                        emb2.AddEmbed(Error("There is no file named `RagePluginHook.log!`"));
                        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb2);
                        return;
                    }
                    break;
            }
            if (_file == null)
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(Error("Failed to load `RagePluginHook.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            if (!_file.Contains("RagePluginHook"))
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(Error("This file is not named `RagePluginHook.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }

            await e.DeferAsync(true);

            //===============================================PROCESS LOG=========================================================
            
            using var client = new WebClient();
            client.DownloadFile(_file,
                Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", Settings.LogNamer()));
            log = LogAnalyzer.Run();
            
            //===============================================VARIABLES=========================================================
            
            var linkedOutdated = log.Outdated.Select(i => i?.Link != null
                    ? $"[{i.DName}]({i.Link})"
                    : $"[{i?.DName}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})")
                .ToList();
            senderID = e.TargetMessage.Author.Id;
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
            
            //===============================================SEND QUICK VIEW=========================================================

            var message = GetBaseLogInfoMessage("# **Quick Log Information**");

            message.AddField("Log uploader:", $"{e.TargetMessage.Author.Mention}", true);
            message.AddField("Log message:", e.TargetMessage.JumpLink.ToString(), true);
            message.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
            
            if (outdated.Length >= 1024 || broken.Length >= 1024)
            {
                message.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.\r\nFor detailed info, first fix the plugins!", true);
                if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
                var message2 = new DiscordEmbedBuilder { Title = ":orange_circle:     **Update:**", Description = "\r\n- " + outdated, Color = DiscordColor.Gold };
                message2.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon };
                var message3 = new DiscordEmbedBuilder { Title = ":red_circle:     **Remove:**", Description = "\r\n- " + broken, Color = DiscordColor.Gold };
                message3.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon };
                
                var overflow = new DiscordWebhookBuilder();
                overflow.AddEmbed(message);
                if (outdated.Length != 0) overflow.AddEmbed(message2);
                if (broken.Length != 0) overflow.AddEmbed(message3);
                overflow.AddComponents(new DiscordComponent[]
                {
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
                    new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info", false, new DiscordComponentEmoji(723417756938010646)),
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
    
    //===============================================SEND DETAILED VIEW=========================================================

    private static async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        
        var message = GetBaseLogInfoMessage("# **Detailed Log Information**");
        
        message.AddField("Log uploader:", $"<@{senderID}>", true);
        message.AddField("Log message:", e.Message.JumpLink.ToString(), true);
        message.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        
        message = AddCommonFields(message);

        foreach (var error in log.Errors)
        {
            message.AddField($"```{error.Level.ToString()} ID: {error.ID}``` Troubleshooting Steps:", $"> {error.Solution}");
        }
            
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, "send2", "Send To User", false, new DiscordComponentEmoji("📨"))
        }));
    }
    
    //===============================================SEND TO USER=========================================================

    private static async Task SendMessageToUser(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseLogInfoMessage(e.Message.Embeds[0].Description);
        
        foreach (var field in e.Message.Embeds[0].Fields)
        {
            if (!field.Name.Contains(":bangbang:")) newEmb.AddField(field.Name, field.Value, field.Inline);
        }
        newEmb.RemoveFieldRange(0, 3);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(e.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.WithContent($"<@{senderID}>");
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithAllowedMention(new UserMention(senderID));
        await e.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(e.Channel);
    }
    
    //===============================================BUTTON MANAGER=========================================================
    
    internal static async Task OnButtonPress(DiscordClient s, ComponentInteractionCreateEventArgs e)
    {
        if (e.Id is "send" or "send2") await SendMessageToUser(e);
        if (e.Id == "info") await SendDetailedInfoMessage(e);
    }
}