using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Modules;

internal class SharedLogInfo
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/b69077cfafb6856a0752c863e1bb87f0.webp?size=128&quality=lossless";
    internal const string OptionValueSeparator = "&";
    internal Guid Guid { get; } = Guid.NewGuid();

    internal async Task SendAttachmentErrorMessage(ContextMenuContext context, string message)
    {
        var response = new DiscordInteractionResponseBuilder
        {
            IsEphemeral = true
        };
        response.AddEmbed(BasicEmbeds.Error(message));
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        return;
    }
    
    internal async Task SendSelectFileForAnalysisMessage(ContextMenuContext context, List<DiscordAttachment> acceptedAttachments)
    {
        DiscordEmbedBuilder embed = BasicEmbeds.Warning(" There were multiple attachments detected for log analysis!\r\n Please select the one you would like to be analyzed!");
        
        List<DiscordSelectComponentOption> selectOptions = new List<DiscordSelectComponentOption>();
        foreach(DiscordAttachment acceptedAttachment in acceptedAttachments)
        {
            string value = context.TargetMessage.Id + OptionValueSeparator + acceptedAttachment.Id.ToString();
            DiscordSelectComponentOption? option = new DiscordSelectComponentOption(acceptedAttachment.FileName, value);
            selectOptions.Add(option);
        }

        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
                new DiscordComponent[]
                {
                    new DiscordSelectComponent(
                        customId: ComponentInteraction.SelectAttachmentForAnalysis,
                        placeholder: "Select",
                        options: selectOptions
                    )
                }
            );  

        var sentMessage = await context.EditResponseAsync(webhookBuilder);
        Program.Cache.SaveProcess(sentMessage.Id, new(context.Interaction, context.TargetMessage));

    }

    internal DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, DiscordMessage originalMessage) 
    {
        embed.AddField("Log uploader:", $"<@{originalMessage.Author.Id}>", true);
        embed.AddField("Log message:", originalMessage.JumpLink.ToString(), true);
        embed.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}