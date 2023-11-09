using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Modules;

internal class LogAnalysisProcess
{
    internal const string TsIcon = "https://cdn.discordapp.com/role-icons/517568233360982017/b69077cfafb6856a0752c863e1bb87f0.webp?size=128&quality=lossless";
    internal Guid Guid { get; }

    public LogAnalysisProcess()
    {
        Guid = Guid.NewGuid();
    }

    internal static async Task SendAttachmentErrorMessage(ContextMenuContext e, string message)
    {
        var response = new DiscordInteractionResponseBuilder
        {
            IsEphemeral = true
        };
        response.AddEmbed(BasicEmbeds.Error(message));
        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        return;
    }
    
    internal static async Task SendSelectFileForAnalysisMessage(ContextMenuContext e, List<DiscordAttachment> acceptedAttachments)
    {
        DiscordEmbedBuilder embed = BasicEmbeds.Warning("There were multiple attachments detected for log analysis. Please select the one you would like to be analyzed!");
        
        List<DiscordSelectComponentOption> selectOptions = new List<DiscordSelectComponentOption>();
        foreach(DiscordAttachment acceptedAttachment in acceptedAttachments)
        {
            string value = e.TargetMessage.Id + "&&" + acceptedAttachment.Id.ToString();
            DiscordSelectComponentOption? option = new DiscordSelectComponentOption(acceptedAttachment.FileName, value);
            selectOptions.Add(option);
        }

        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(
                new DiscordComponent[]
                {
                    new DiscordSelectComponent(
                        customId: "selectAttachmentForAnalysis",
                        placeholder: "Select",
                        options: selectOptions
                    )
                }
            );  

        await e.EditResponseAsync(webhookBuilder);
    }

    internal static DiscordEmbedBuilder AddTsViewFields(DiscordEmbedBuilder embed, ulong messageId) 
    {
        ProcessCache cache = Program.Cache.GetProcessCache(messageId);

        embed.AddField("Log uploader:", $"<@{cache.OriginalMessage.Author.Id}>", true);
        embed.AddField("Log message:", cache.OriginalMessage.JumpLink.ToString(), true);
        embed.AddField("\u200B", "\u200B", true); //TS View only! Always index 0 - 2.
        return embed;
    }

    internal static DiscordEmbedBuilder RemoveTsViewFields(DiscordEmbedBuilder embed)
    {
        return embed.RemoveFieldRange(0, 3);
    }
}