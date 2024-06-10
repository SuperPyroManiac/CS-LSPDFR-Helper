using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules;

internal class SharedLogInfo
{
    internal const string OptionValueSeparator = "&";

    // ReSharper disable once ReplaceAsyncWithTaskReturn
    internal async Task SendAttachmentErrorMessage(ContextMenuInteractionCreatedEventArgs context, string message)
    {
        var response = new DiscordInteractionResponseBuilder
        {
            IsEphemeral = true
        };
        response.AddEmbed(BasicEmbeds.Error(message));
        await context.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }
    
    internal async Task SendSelectFileForAnalysisMessage(ContextMenuInteractionCreatedEventArgs context, List<DiscordAttachment> acceptedAttachments)
    {
        var embed = BasicEmbeds.Warning(" There were multiple attachments detected for log analysis!\r\n Please select the one you would like to be analyzed!");
        
        List<DiscordSelectComponentOption> selectOptions = [];
        foreach(var acceptedAttachment in acceptedAttachments)
        {
            var value = context.TargetMessage.Id + OptionValueSeparator + acceptedAttachment.Id;
            var option = new DiscordSelectComponentOption(acceptedAttachment.FileName, value);
            selectOptions.Add(option);
        }

        var webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
	            // ReSharper disable once RedundantExplicitParamsArrayCreation
                [
                    new DiscordSelectComponent(
                        customId: ComponentInteraction.SelectAttachmentForAnalysis,
                        placeholder: "Select",
                        options: selectOptions
                    )
                ]
            );  

        var sentMessage = await context.Interaction.EditOriginalResponseAsync(webhookBuilder);
        Program.Cache.SaveProcess(sentMessage.Id, new(context.Interaction, context.TargetMessage));

    }

    internal DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, ProcessCache cache, Log log) 
    {
        embed.AddField("Log uploader:", $"<@{cache.OriginalMessage.Author.Id}>", true);
        embed.AddField("Log message:", cache.OriginalMessage.JumpLink.ToString(), true);
        embed.AddField("Elapsed time:", $"{log.ElapsedTime}ms", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}