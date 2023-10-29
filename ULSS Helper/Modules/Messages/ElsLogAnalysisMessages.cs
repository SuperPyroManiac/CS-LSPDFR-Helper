using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Messages;
internal class ElsLogAnalysisMessages : LogAnalysisMessages
{
    internal static AnalyzedElsLog log;

    internal static async Task SendQuickLogInfoMessage(ContextMenuContext e)
    {
        logUploaderUserId = e.TargetMessage.Author.Id;
        logMessageLink = e.TargetMessage.JumpLink.ToString();

        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("### Quick ELS.log Info");

        embed = AddTsViewFields(embed);

        if (log.FaultyVcfFile != null) 
        {
            embed.AddField($":red_circle:     Remove the following faulty VCF file from `{log.VcfContainer}`", log.FaultyVcfFile);
        }
        else 
        {
            embed.AddField($":green_circle     No faulty VCF file detected!", "Seems like ELS loaded fine and didn't cause a crash.");
        }

        if (log.TotalAmountElsModels != null) 
            embed.AddField("Total amount of ELS-enabled models:", log.TotalAmountElsModels.ToString());

        DiscordWebhookBuilder message = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
                new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "elsDetails", "More Info", false, new DiscordComponentEmoji(723417756938010646)),
                    new DiscordButtonComponent(ButtonStyle.Danger, "sendElsToUser", "Send To User", false, new DiscordComponentEmoji("📨"))
                }
            );

        await e.EditResponseAsync(message);
    }


    internal static async Task SendDetailedInfoMessage(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        
        DiscordEmbedBuilder embed = GetBaseLogInfoEmbed("### Detailed ELS.log Info");
        
        //embed = AddTsViewFields(embed);

        foreach (var field in e.Message.Embeds[0].Fields)
        {
            embed.AddField(field.Name, field.Value, field.Inline);
        }

        if (log.VcfContainer != null)
            embed.AddField("VCF Container Location:", log.VcfContainer);

        string validVcFiles = "\r\n- " + string.Join("\r\n- ", log.ValidElsVcfFiles);
        if (log.ValidElsVcfFiles.Count > 0)
            embed.AddField(":green_circle:     Valid VCFs:", validVcFiles, true);

        string invalidVcFiles = "\r\n- " + string.Join("\r\n- ", log.InvalidElsVcfFiles);
        if (log.InvalidElsVcfFiles.Count > 0) 
            embed.AddField(":orange_circle:     Invalid VCFs:", invalidVcFiles, true);
        
            
        await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
        {
            new DiscordButtonComponent(ButtonStyle.Danger, "sendElsDetailsToUser", "Send To User", false, new DiscordComponentEmoji("📨"))
        }));
    }

    internal static async Task SendMessageToUser(ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.DeferAsync(true);
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseLogInfoEmbed(e.Message.Embeds[0].Description);
        
        foreach (var field in e.Message.Embeds[0].Fields)
        {
            newEmb.AddField(field.Name, field.Value, field.Inline);
        }
        newEmb = RemoveTsViewFields(newEmb);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(e.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.WithContent($"<@{logUploaderUserId}>");
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithAllowedMention(new UserMention(logUploaderUserId));
        await e.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(e.Channel);
    }

    private static DiscordEmbedBuilder GetBaseLogInfoEmbed(string description) 
    {
        return new DiscordEmbedBuilder
        {
            Description = description,
            Color = DiscordColor.Gold,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = TsIcon },
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = $"ELS Version: {log.ElsVersion} - AdvancedHookV installed: {(log.AdvancedHookVFound ? "\u2713" : "X")}"
            }
        };
    }
}