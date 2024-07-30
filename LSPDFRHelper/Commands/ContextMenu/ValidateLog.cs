using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Processors.ASI;
using LSPDFRHelper.Functions.Processors.ELS;
using LSPDFRHelper.Functions.Processors.RPH;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.ContextMenu;

public class ValidateLog
{
    [Command("Validate Log")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    public async Task ValidateLogCmd(SlashCommandContext ctx, DiscordMessage targetMessage)
    {
        if (!await PermissionManager.RequireNotBlacklisted(ctx)) return;
            
        //===//===//===////===//===//===////===//Attachment Checks/===////===//===//===////===//===//===//
        DiscordAttachment attach = null;
        List<string> acceptedFileNames = ["RagePluginHook", "ELS", "asiloader", "ScriptHookVDotNet"];
        var response = new DiscordInteractionResponseBuilder();
        response.IsEphemeral = true;
        
        
        switch (targetMessage.Attachments.Count)
        {
            case 0:
                await ctx.RespondAsync(response.AddEmbed(BasicEmbeds.Error("__No File Found!__\r\n>>> The selected message must include a valid log type!\r\n- RagePluginHook.log\r\n- ELS.log\r\n- ScriptHookVDotNet.log\r\n- asiloader.log")));
                return;
            case 1:
                attach = targetMessage.Attachments[0];
                break;
            case > 1:
                List<DiscordAttachment> acceptedAttachments = [];
                acceptedAttachments.AddRange(targetMessage.Attachments.Where(attachment => acceptedFileNames.Any(attachment.FileName!.Contains)));
                switch (acceptedAttachments.Count)
                {
                    case 0:
                        await ctx.RespondAsync(response.AddEmbed(BasicEmbeds.Error("__No Valid File Found!__\r\n>>> The selected message must include a valid log type!\r\n- RagePluginHook.log\r\n- ELS.log\r\n- ScriptHookVDotNet.log\r\n- asiloader.log")));
                        return;
                    case 1:
                        attach = acceptedAttachments[0];
                        break;
                    case > 1:
                        await ctx.Interaction.DeferAsync(true);
                        var embed = BasicEmbeds.Warning("__Multiple Valid Files!__\r\n Please select the one you would like to be validated!");
        
                        List<DiscordSelectComponentOption> selectOptions = [];
                        foreach(var acceptedAttachment in acceptedAttachments)
                        {
                            var value = targetMessage.Id + "&" + acceptedAttachment.Id;
                            var option = new DiscordSelectComponentOption(acceptedAttachment.FileName!, value);
                            selectOptions.Add(option);
                        }

                        var webhookBuilder = new DiscordWebhookBuilder()
                            .AddEmbed(embed)
                            .AddComponents(
                                [
                                    new DiscordSelectComponent(
                                        customId: CustomIds.SelectAttachmentForAnalysis,
                                        placeholder: "Select",
                                        options: selectOptions
                                    )
                                ]
                            );  

                        var sentMessage = await ctx.EditResponseAsync(webhookBuilder);
                        Program.Cache.SaveProcess(sentMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage));
                        return;
                } 
                break;
        }
        if (attach == null)
        {
            await ctx.RespondAsync(response.AddEmbed(BasicEmbeds.Error("__Failed To Find File!__\r\n>>> The selected message must include a valid log type!\r\n- RagePluginHook.log\r\n- ELS.log\r\n- ScriptHookVDotNet.log\r\n- asiloader.log")));
            return;
        }
        if (!acceptedFileNames.Any(attach.FileName!.Contains))
        {
            await ctx.RespondAsync(response.AddEmbed(BasicEmbeds.Error("__No Valid File Found!__\r\n>>> The selected message must include a valid log type!\r\n- RagePluginHook.log\r\n- ELS.log\r\n- ScriptHookVDotNet.log\r\n- asiloader.log")));
            return;
        }
        
        //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
        if (attach.FileName.Contains("RagePluginHook"))
        {
            await ctx.Interaction.DeferAsync(true);
            var rphProcessor = new RphProcessor();
            var cache = Program.Cache.GetProcess(targetMessage.Id);
            if (ProcessCache.IsCacheUsagePossible("RagePluginHook", cache)) rphProcessor = cache.RphProcessor;
            else
            {
                rphProcessor.Log = await RPHValidater.Run(attach.Url);
                rphProcessor.Log.MsgId = targetMessage.Id;
                //TODO: ProxyCheck.Run(rphProcess.log, Program.Cache.GetUser(targetMessage.Author!.Id.ToString()), targetMessage);
                Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, rphProcessor));
            }
            await rphProcessor.SendQuickInfoMessage(targetMessage, ctx);
            return;
        }
        
        if (attach.FileName.Contains("ELS"))
        {
            await ctx.Interaction.DeferAsync(true);
            var elsProcessor = new ELSProcessor();
            var cache = Program.Cache.GetProcess(targetMessage.Id);
            if (ProcessCache.IsCacheUsagePossible("ELS", cache)) elsProcessor = cache.ElsProcessor;
            else
            {
                elsProcessor.Log = await ELSValidater.Run(attach.Url);
                elsProcessor.Log.MsgId = targetMessage.Id;
                Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, elsProcessor));
            }
            await elsProcessor.SendQuickInfoMessage(targetMessage, ctx);
            return;
        }
        
        if (attach.FileName.Contains("asiloader"))
        {
            await ctx.Interaction.DeferAsync(true);
            var asiProcessor = new ASIProcessor();
            var cache = Program.Cache.GetProcess(targetMessage.Id);
            if (ProcessCache.IsCacheUsagePossible("ASI", cache)) asiProcessor = cache.AsiProcessor;
            else
            {
                asiProcessor.Log = await ASIValidater.Run(attach.Url);
                asiProcessor.Log.MsgId = targetMessage.Id;
                Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, asiProcessor));
            }
            await asiProcessor.SendQuickInfoMessage(targetMessage, ctx);
            return;
        }
    }
}