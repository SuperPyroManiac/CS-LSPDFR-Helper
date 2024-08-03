using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.CustomTypes.LogTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Processors.ASI;

public class ASIProcessor : SharedData
{
    public ASILog Log;
    
    private DiscordEmbedBuilder GetBaseEmbed(string description)
    {
        return BasicEmbeds.Ts(description,
            new DiscordEmbedBuilder.EmbedFooter { Text = $"Loaded ASIs: {Log.LoadedAsiFiles.Count} - Failed ASIs: {Log.FailedAsiFiles.Count}" });
    }
    
    public async Task SendQuickInfoMessage(DiscordMessage targetMessage = null, CommandContext context=null, ComponentInteractionCreatedEventArgs eventArgs=null)
    {
        if (context == null && eventArgs == null) throw new InvalidDataException("Parameters 'context' and 'eventArgs' can not both be null!");
        if (targetMessage == null) targetMessage = eventArgs!.Message;

        var embed = GetBaseEmbed("## ASI.log Info");
        
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        embed = AddTsViewFields(embed, cache, Log);
        
        if ( Log.BrokenAsiFiles.Count > 0) embed.AddField(":red_circle:     Remove these known broken ASI files:", ">>> - " + string.Join("\r\n- ", Log.BrokenAsiFiles));

        if (Log.FailedAsiFiles.Count != 0) 
        {
            embed.AddField(":red_circle:     Some ASIs failed to load!", "See below for details!");
            List<string> failedNames = [];
            failedNames.AddRange(Log.FailedAsiFiles.Select(asi => asi.Name));
            var failedASIs = ">>> " + string.Join(" - ", failedNames);
            if ( failedASIs.Length < 1024 ) embed.AddField("Failed:", failedASIs);
            else embed.AddField("Too many to show!", $">>> Manually review the log to see all `{Log.FailedAsiFiles.Count}` failed ASI's!");

            if ( Log.FailedAsiFiles.Any(x => x.Name == "ELS.asi") ) embed.AddField("Possible ELS Issue!", ">>> Ensure that you have installed both [AdvancedHookV.dll](https://www.lcpdfr.com/downloads/gta5mods/scripts/13865-emergency-lighting-system/) & [ScriptHookV](http://dev-c.com/GTAV/scripthookv)!");
            if ( Log.FailedAsiFiles.Any(x => x.PluginType == PluginType.SHV) ) embed.AddField("Possible SHV Issue!", ">>> Ensure you have installed [ScriptHookV](http://dev-c.com/GTAV/scripthookv)!");

        }
        else embed.AddField(":green_circle:     No faulty ASI files detected!", ">>> Seems like everything loaded fine.");
        

        var webhookBuilder = new DiscordWebhookBuilder()
            .AddEmbed(embed)
            .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Danger, CustomIds.AsiSendToUser, "Send To User", false, new DiscordComponentEmoji("📨")));

        DiscordMessage sentMessage;
        if (context != null) sentMessage = await context.EditResponseAsync(webhookBuilder);
        else sentMessage = await eventArgs.Interaction.EditOriginalResponseAsync(webhookBuilder);
        
        if (Log.Missing.Count > 0) 
            await SendUnknownPluginsLog(cache.OriginalMessage.Channel!.Id, cache.OriginalMessage.Author!.Id, Log.DownloadLink, Log.Missing, []);
            
        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(cache.Interaction, cache.OriginalMessage, this));
    }

    public async Task SendMessageToUser(ComponentInteractionCreatedEventArgs eventArgs)
    {
        var newEmbList = new List<DiscordEmbed>();
        var newEmb = GetBaseEmbed(eventArgs.Message.Embeds[0].Description);
        
        foreach (var field in eventArgs.Message.Embeds[0].Fields!)
        {
            newEmb.AddField(field.Name!, field.Value!, field.Inline);
        }
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
    
    public async Task SendAutoHelperMessage(MessageCreatedEventArgs ctx)
    {
        var embed = GetBaseEmbed("## ASI.log Info");
        
        if ( Log.BrokenAsiFiles.Count > 0) embed.AddField(":red_circle:     Remove these known broken ASI files:", ">>> - " + string.Join("\r\n- ", Log.BrokenAsiFiles));

        if (Log.FailedAsiFiles.Count != 0) 
        {
            embed.AddField(":red_circle:     Some ASIs failed to load!", "See below for details!");
            List<string> failedNames = [];
            failedNames.AddRange(Log.FailedAsiFiles.Select(asi => asi.Name));
            var failedASIs = ">>> " + string.Join(" - ", failedNames);
            if ( failedASIs.Length < 1024 ) embed.AddField("Failed:", failedASIs);
            else embed.AddField("Too many to show!", $">>> Manually review the log to see all `{Log.FailedAsiFiles.Count}` failed ASI's!");

            if ( Log.FailedAsiFiles.Any(x => x.Name == "ELS.asi") ) embed.AddField("Possible ELS Issue!", ">>> Ensure that you have installed both [AdvancedHookV.dll](https://www.lcpdfr.com/downloads/gta5mods/scripts/13865-emergency-lighting-system/) & [ScriptHookV](http://dev-c.com/GTAV/scripthookv)!");
            if ( Log.FailedAsiFiles.Any(x => x.PluginType == PluginType.SHV) ) embed.AddField("Possible SHV Issue!", ">>> Ensure you have installed [ScriptHookV](http://dev-c.com/GTAV/scripthookv)!");

        }
        else embed.AddField(":green_circle:     No faulty ASI files detected!", ">>> Seems like everything loaded fine.");
        
        await ctx.Message.RespondAsync(new DiscordMessageBuilder().AddEmbed(embed));
        
        if (Log.Missing.Count > 0) 
            await SendUnknownPluginsLog(ctx.Message.Channel!.Id, ctx.Message.Author!.Id, Log.DownloadLink, Log.Missing, []);
    }
}