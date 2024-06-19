using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Events;

public class ContextMenu
{    
    [Command("Analyze Log")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    public async Task OnMenuSelect(SlashCommandContext context, DiscordMessage targetMessage)
    {
        if (!await PermissionManager.RequireTs(context)) return;
        //===//===//===////===//===//===////===//Attachment Checks/===////===//===//===////===//===//===//
        DiscordAttachment attachmentForAnalysis = null;
        List<string> acceptedFileNames = [..new[] { "RagePluginHook", "ELS", "asiloader", "ScriptHookVDotNet" }];
        var acceptedFileNamesString = string.Join(" or ", acceptedFileNames);
        var acceptedLogFileNamesString = "`" + string.Join(".log` or `", acceptedFileNames) + ".log`";
        SharedLogInfo sharedLogInfo = new();
        try
        {
            switch (targetMessage.Attachments.Count)
            {
                case 0:
                    await sharedLogInfo.SendAttachmentErrorMessage(context, $"No attachment found. There needs to be a {acceptedFileNamesString} log file attached to the message!");
                    return;
                case 1:
                    attachmentForAnalysis = targetMessage.Attachments[0];
                    break;
                case > 1:
                    List<DiscordAttachment> acceptedAttachments = [];
                    foreach(var attachment in targetMessage.Attachments)
                    {
                        if (acceptedFileNames.Any(attachment.FileName.Contains))
                        {
                            acceptedAttachments.Add(attachment);
                        }
                    }
                    switch (acceptedAttachments.Count)
                    {
                        case 0:
                            await sharedLogInfo.SendAttachmentErrorMessage(context, $"There is no file named {acceptedLogFileNamesString} attached!");
                            return;
                        case 1:
                            attachmentForAnalysis = acceptedAttachments[0];
                            break;
                        case > 1:
                            await context.Interaction.DeferAsync(true);
                            await sharedLogInfo.SendSelectFileForAnalysisMessage(context, acceptedAttachments, targetMessage);
                            return;
                    }
                    break;
            }
            if (attachmentForAnalysis == null)
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, "Failed to load attached file!");
                return;
            }
            if (!acceptedFileNames.Any(attachmentForAnalysis.FileName!.Contains))
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, $"This file is not named {acceptedLogFileNamesString}!");
                return;
            }
            
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (attachmentForAnalysis.FileName.Contains("RagePluginHook"))
            {
                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
	            var th = new Thread(() => RphThread(context, attachmentForAnalysis, targetMessage));
	            th.Start();
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("ELS"))
            {
                var th = new Thread(() => ElsThread(context, attachmentForAnalysis, targetMessage));
                th.Start();
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("asiloader"))
            {
                var th = new Thread(() => AsiThread(context, attachmentForAnalysis, targetMessage));
                th.Start();
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("ScriptHookVDotNet"))
            {
                var th = new Thread(() => ShvdnThread(context, attachmentForAnalysis, targetMessage));
                th.Start();
            }
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
    
    private async Task RphThread(SlashCommandContext context, DiscordAttachment attachmentForAnalysis, DiscordMessage targetMessage)
    {
        await context.Interaction.DeferAsync(true);
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        RPHProcess rphProcess;
        if (ProcessCache.IsCacheUsagePossible("RagePluginHook", cache))
            rphProcess = cache.RphProcess;
        else 
        {
            rphProcess = new RPHProcess();
            rphProcess.log = await RPHAnalyzer.Run(attachmentForAnalysis.Url);
            rphProcess.log.MsgId = targetMessage.Id;
            ProxyCheck.Run(rphProcess.log, Program.Cache.GetUser(targetMessage.Author!.Id.ToString()), targetMessage);
            Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, rphProcess));
        }
        
        await rphProcess.SendQuickLogInfoMessage(targetMessage, context);
    }

    private async Task ElsThread(SlashCommandContext context, DiscordAttachment attachmentForAnalysis, DiscordMessage targetMessage)
    {
        context.Interaction.DeferAsync(true);
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        ELSProcess elsProcess;
        if (ProcessCache.IsCacheUsagePossible("ELS", cache))
            elsProcess = cache.ElsProcess;
        else
        {
            elsProcess = new ELSProcess();
            elsProcess.log = await ELSAnalyzer.Run(attachmentForAnalysis.Url);
            elsProcess.log.MsgId = targetMessage.Id;
            Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, elsProcess));
        }
        
        await elsProcess.SendQuickLogInfoMessage(targetMessage, context);
    }

    private async Task AsiThread(SlashCommandContext context, DiscordAttachment attachmentForAnalysis, DiscordMessage targetMessage)
    {
        context.Interaction.DeferAsync(true);
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        ASIProcess asiProcess;
        if (ProcessCache.IsCacheUsagePossible("asiloader", cache))
            asiProcess = cache.AsiProcess;
        else 
        {
            asiProcess = new ASIProcess();
            asiProcess.log = await ASIAnalyzer.Run(attachmentForAnalysis.Url);
            asiProcess.log.MsgId = targetMessage.Id;
            Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, asiProcess));
        }
        
        await asiProcess.SendQuickLogInfoMessage(targetMessage, context);
    }

    private async Task ShvdnThread(SlashCommandContext context, DiscordAttachment attachmentForAnalysis, DiscordMessage targetMessage)
    {
        await context.Interaction.DeferAsync(true);
        var cache = Program.Cache.GetProcess(targetMessage.Id);
        SHVDNProcess shvdnProcess;
        if (ProcessCache.IsCacheUsagePossible("ScriptHookVDotNet", cache))
            shvdnProcess = cache.ShvdnProcess;
        else
        {
            shvdnProcess = new SHVDNProcess();
            shvdnProcess.log = await SHVDNAnalyzer.Run(attachmentForAnalysis.Url);
            shvdnProcess.log.MsgId = targetMessage.Id;
            Program.Cache.SaveProcess(targetMessage.Id, new ProcessCache(targetMessage.Interaction, targetMessage, shvdnProcess));
        }

        await shvdnProcess.SendQuickLogInfoMessage(targetMessage, context);
    }
}