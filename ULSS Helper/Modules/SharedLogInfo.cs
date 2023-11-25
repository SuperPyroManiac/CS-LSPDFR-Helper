using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules;

internal class SharedLogInfo
{
    internal const string OptionValueSeparator = "&";
    internal static TimeSpan ProcessRestartCooldown = TimeSpan.FromMinutes(1); // the minimum age for a log analysis process object before it can be overwritten by a new process for the same log (restart).

    // ReSharper disable once ReplaceAsyncWithTaskReturn
    internal async Task SendAttachmentErrorMessage(ContextMenuContext context, string message)
    {
        var response = new DiscordInteractionResponseBuilder
        {
            IsEphemeral = true
        };
        response.AddEmbed(BasicEmbeds.Error(message));
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
    
    internal async Task SendSelectFileForAnalysisMessage(ContextMenuContext context, List<DiscordAttachment> acceptedAttachments)
    {
        DiscordEmbedBuilder embed = BasicEmbeds.Warning(" There were multiple attachments detected for log analysis!\r\n Please select the one you would like to be analyzed!");
        
        List<DiscordSelectComponentOption> selectOptions = new List<DiscordSelectComponentOption>();
        foreach(DiscordAttachment acceptedAttachment in acceptedAttachments)
        {
            string value = context.TargetMessage.Id + OptionValueSeparator + acceptedAttachment.Id;
            DiscordSelectComponentOption option = new DiscordSelectComponentOption(acceptedAttachment.FileName, value);
            selectOptions.Add(option);
        }

        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
	            // ReSharper disable once RedundantExplicitParamsArrayCreation
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

    internal DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, ProcessCache cache, string elapsedTime) 
    {
        DateTime cacheExpiry = cache.ModifiedAt.AddMinutes(ProcessRestartCooldown.Minutes);
        string expiryFormatted = Formatter.Timestamp(cacheExpiry);
        embed.AddField("Log uploader:", $"<@{cache.OriginalMessage.Author.Id}>", true);
        embed.AddField("Log message:", cache.OriginalMessage.JumpLink.ToString(), true);
        embed.AddField("Elapsed time:", $"{elapsedTime}ms\r\n||Cooldown expiry: {expiryFormatted}||", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}