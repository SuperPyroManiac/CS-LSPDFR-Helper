using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules;

internal class ContextManager : ApplicationCommandModule
{
    private static string? _file;
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    public static async Task OnMenuSelect(ContextMenuContext e)
    {
        if (e.Member.Roles.All(role => role.Id != 517568233360982017))
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
                case > 1:
                    var emb = new DiscordInteractionResponseBuilder();
                    emb.IsEphemeral = true;
                    emb.AddEmbed(MessageManager.Error("There can only be 1 attachment; named RagePluginHook.log!"));
                    await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                    return;
            }
            _file = e.TargetMessage.Attachments[0]?.Url;
            if (_file == null)
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(MessageManager.Error("Failed to load RagePluginHook.log!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            if (!_file.Contains("RagePluginHook.log"))
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(MessageManager.Error("This file is not named RagePluginHook.log!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            await e.DeferAsync(true);

            //========================================================================================================
            
            using var client = new WebClient();
            client.DownloadFile(_file,
                Path.Combine(Directory.GetCurrentDirectory(), "RPHLogs", Settings.LogNamer())); //Download the file
            var log = LogAnalyzer.Run(); //Process the log
            
            var linkedOutdated = new List<string>();
            foreach (var i in log.Outdated)
            {
                if (i.Link != null)
                {
                    linkedOutdated.Add($"[{i.DName}]({i.Link})");
                }
                else
                {
                    linkedOutdated.Add($"[{i}](https://www.google.com/search?q=lspdfr+{i.DName.Replace(" ", "+")})");
                }
            }

            var curentList = log.Current.Select(i => i?.DName).ToList();
            var brokenList = log.Broken.Select(i => i?.DName).ToList();
            var missingList = log.Missing.Select(i => i?.Name).ToList();
            var libraryList = log.Library.Select(i => i?.DName).ToList();
            brokenList.AddRange(libraryList);
            var current = string.Join("\r\n- ", curentList);
            var outdated = string.Join("\r\n- ", linkedOutdated);
            var broken = string.Join("\r\n- ", brokenList);
            var missing = string.Join(", ", missingList);
            var library = string.Join(", ", libraryList);
            var GTAver = "X";
            var LSPDFRver = "X";
            var RPHver = "X";

            if (Settings.GTAVer == log.GTAVersion) GTAver = "\u2713";
            if (Settings.LSPDFRVer == log.LSPDFRVersion) LSPDFRver = "\u2713";
            if (Settings.RPHVer == log.RPHVersion) RPHver = "\u2713";

            var message = new DiscordEmbedBuilder();
            message.Description = "# **Quick Log Information**";
            message.Color = DiscordColor.Gold;
            message.Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = e.TargetMessage.Author.Username, IconUrl = e.TargetMessage.Author.AvatarUrl};
            message.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless" };
            message.Footer = new DiscordEmbedBuilder.EmbedFooter()
             {
                 Text = $"GTA: {GTAver} - RPH: {RPHver} - LSPDFR: {LSPDFRver}"
             };
            
            if (outdated.Length >= 1024 || broken.Length >= 1024)
            {
                message.AddField(":warning:     **Message Too Big**", "\r\nToo many plugins to display in a single message.", true);
                if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
                var message2 = new DiscordEmbedBuilder { Title = ":orange_circle:     **Outdated Plugins**", Description = "\r\n- " + outdated, Color = DiscordColor.Gold };
                message2.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless" };
                var message3 = new DiscordEmbedBuilder { Title = ":red_circle:     **Broken Plugins**", Description = "\r\n- " + broken, Color = DiscordColor.Gold };
                message3.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "https://cdn.discordapp.com/role-icons/517568233360982017/645944c1c220c8121bf779ea2e10b7be.webp?size=128&quality=lossless" };
                
                var overflow = new DiscordWebhookBuilder();
                overflow.AddEmbed(message);
                if (outdated.Length != 0) overflow.AddEmbed(message2);
                if (broken.Length != 0) overflow.AddEmbed(message3);
                overflow.AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info (WIP)", true,
                        new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, "send", "Send To User", false,
                        new DiscordComponentEmoji("📨"))
                });
                await e.EditResponseAsync(overflow);
            }
            else
            {
                if (current.Length != 0 && outdated.Length == 0 && broken.Length != 0) outdated = "**None**";
                if (current.Length != 0 && outdated.Length != 0 && broken.Length == 0) broken = "**None**";

                if (outdated.Length > 0) message.AddField(":orange_circle:     **Update:**", "\r\n- " + outdated, true);
                if (broken.Length > 0) message.AddField(":red_circle:     **Remove:**", "\r\n- " + broken, true);
                if (missing.Length > 0) message.AddField(":bangbang:  **Plugins not recognized:**", missing, false);
            
                if (current.Length > 0 && outdated.Length == 0 && broken.Length == 0) message.AddField(":green_circle:     **No outdated or broken plugins!**", "- All up to date!");
                
                if (log.LSPDFRVersion == null) message.AddField(":red_circle:     **LSPDFR Not Loaded!**", "\r\n- **You should manually check the log!**");
                
                if (current.Length == 0 && outdated.Length == 0 && broken.Length == 0 && log.LSPDFRVersion != null) message.AddField(":green_circle:     **No installed plugins!**", "- Can't have plugin issues if you don't got any!");

                
                await e.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(message).AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "info", "More Info (WIP)", true, new DiscordComponentEmoji(723417756938010646)),
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
        if (e.Id == "send")
        {
            await e.Interaction.DeferAsync();
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbeds(e.Message.Embeds));
        }

        if (e.Id == "info")
        {
            
        }
    }
}