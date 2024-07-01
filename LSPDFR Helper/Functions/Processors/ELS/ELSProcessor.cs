using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.LogTypes;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions.Processors.ELS;

public class ELSProcessor : SharedData
{
    public ELSLog Log;

    private DiscordEmbedBuilder GetBaseEmbed(string description)
    {
        return BasicEmbeds.Ts(description,
            new DiscordEmbedBuilder.EmbedFooter { Text = $"ELS Version: {Log.ElsVersion} - AdvancedHookV installed: {( Log.AdvancedHookVFound ? "\u2713" : "❌" )}" });
    }

    public async Task SendQuickInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null) throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        if (targetMessage == null) targetMessage = eventArgs!.Message;

        var embed = GetBaseEmbed("## ELS.log Info");

        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, Log);

        if (Log.FaultyVcfFile != null) 
        {
            embed.AddField(":red_circle:     Faulty VCF found!", $">>> `ELS/pack_default/{Log.FaultyVcfFile}` is faulty and should be removed!");
            Log.ValidElsVcfFiles.Clear();
            Log.ValidElsVcfFiles.Add("\r\n__**Cannot show until the fault is fixed!**__");
        }
        else switch (Log.FaultyVcfFile)
        {
            case null when Log.InvalidElsVcfFiles.Count > 0:
                embed.AddField(":orange_circle:     No serious issues detected!", ">>> No real issues found. Though you have multiple unused VCF's installed! You can ignore or remove these from: `ELS/pack_default/`");
                var invalidVcFiles = ">>> " + string.Join(" - ", Log.InvalidElsVcfFiles);
                if ( invalidVcFiles.Length < 1024 ) embed.AddField("Unused:", invalidVcFiles);
                else embed.AddField("Too many to show!", $">>> Manually review the log to see all `{Log.InvalidElsVcfFiles.Count}` unused VCF's!");
                break;
            case null when Log.InvalidElsVcfFiles.Count == 0:
                embed.AddField(":green_circle:     No issues detected!", ">>> This log shows no issues. If you still have ELS issues, try checking the ASILoader.log!");
                break;
        }

        var webhookBuilder = new DiscordWebhookBuilder();
        webhookBuilder.AddEmbed(embed);
        webhookBuilder.AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.ElsSendToUser, "Send To User", false, new DiscordComponentEmoji("📨")));

        DiscordMessage sentMessage;
        if (context != null) sentMessage = await context.EditResponseAsync(webhookBuilder);
        else sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
            
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this));
    }

    public async Task SendMessageToUser(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseEmbed(eventArgs.Message.Embeds[0].Description);
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields!)
            newEmb.AddField(field.Name!, field.Value!, field.Inline);
        
        newEmb = RemoveTsViewFields(newEmb);
        newEmbList.Add(newEmb);
        newEmbList.AddRange(eventArgs.Message.Embeds);
        newEmbList.RemoveAt(1);

        var newMessage = new DiscordMessageBuilder();
        newMessage.AddEmbeds(newEmbList);
        newMessage.WithReply(Log.MsgId, true);
        await eventArgs.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
            new DiscordInteractionResponseBuilder().AddEmbed(BasicEmbeds.Info("Sent!")));
        await eventArgs.Interaction.DeleteOriginalResponseAsync();
        await newMessage.SendAsync(eventArgs.Channel);
    }
}