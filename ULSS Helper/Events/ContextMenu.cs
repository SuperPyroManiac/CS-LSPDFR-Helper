﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ASI_Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;
using ULSS_Helper.Modules.SHVDN_Modules;

namespace ULSS_Helper.Events;

internal class ContextMenu : ApplicationCommandModule
{
    private static DiscordAttachment? _attachmentForAnalysis;
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    public async Task OnMenuSelect(ContextMenuContext context)
    {
        //===//===//===////===//===//===////===//Permissions/===////===//===//===////===//===//===//
        if (context.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(BasicEmbeds.Error("You do not have permission for this!"));
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }

        //===//===//===////===//===//===////===//Attachment Checks/===////===//===//===////===//===//===//
        _attachmentForAnalysis = null;
        List<string> acceptedFileNames = new(new string[]{ "RagePluginHook", "ELS", "asiloader", "ScriptHookVDotNet" });
        string acceptedFileNamesString = string.Join(" or ", acceptedFileNames);
        string acceptedLogFileNamesString = "`" + string.Join(".log` or `", acceptedFileNames) + ".log`";
        SharedLogInfo sharedLogInfo = new();
        try
        {
            switch (context.TargetMessage.Attachments.Count)
            {
                case 0:
                    await sharedLogInfo.SendAttachmentErrorMessage(context, $"No attachment found. There needs to be a {acceptedFileNamesString} log file attached to the message!");
                    return;
                case 1:
                    _attachmentForAnalysis = context.TargetMessage.Attachments[0];
                    break;
                case > 1:
                    List<DiscordAttachment> acceptedAttachments = new List<DiscordAttachment>();
                    foreach(DiscordAttachment attachment in context.TargetMessage.Attachments)
                    {
                        if (acceptedFileNames.Any(attachment.FileName.Contains))
                        {
                            acceptedAttachments.Add(attachment);
                        }
                    }
                    if (acceptedAttachments.Count == 0)
                    {
                        await sharedLogInfo.SendAttachmentErrorMessage(context, $"There is no file named {acceptedLogFileNamesString} attached!");
                        return;
                    }
                    else if (acceptedAttachments.Count == 1) 
                        _attachmentForAnalysis = acceptedAttachments[0];
                    else if (acceptedAttachments.Count > 1)
                    {
                        await context.DeferAsync(true);
                        await sharedLogInfo.SendSelectFileForAnalysisMessage(context, acceptedAttachments);
                        return;
                    }
                    break;
            }
            if (_attachmentForAnalysis == null)
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, "Failed to load attached file!");
                return;
            }
            if (!acceptedFileNames.Any(_attachmentForAnalysis.FileName.Contains))
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, $"This file is not named {acceptedLogFileNamesString}!");
                return;
            }
            
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (_attachmentForAnalysis.FileName.Contains("RagePluginHook"))
            {
                var th = new Thread(() => RPHThread(context, _attachmentForAnalysis));
                th.Start();
                return;
            }
            if (_attachmentForAnalysis.FileName.Contains("ELS"))
            {
                var th = new Thread(() => ELSThread(context, _attachmentForAnalysis));
                th.Start();
                return;
            }
            if (_attachmentForAnalysis.FileName.Contains("asiloader"))
            {
                var th = new Thread(() => ASIThread(context, _attachmentForAnalysis));
                th.Start();
                return;
            }
            if (_attachmentForAnalysis.FileName.Contains("ScriptHookVDotNet"))
            {
                var th = new Thread(() => SHVDNThread(context, _attachmentForAnalysis));
                th.Start();
                return;
            }
        }
        catch (Exception exception)
        {
            Logging.ErrLog(exception.ToString());
            Console.WriteLine(exception);
            throw;
        }
    }
    
    private async Task RPHThread(ContextMenuContext context, DiscordAttachment attachmentForAnalysis)
    {
        await context.DeferAsync(true);
        RPHProcess rphProcess = new RPHProcess();
        rphProcess.log = RPHAnalyzer.Run(attachmentForAnalysis.Url);
        rphProcess.log.MsgId = context.TargetMessage.Id;
        Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, rphProcess));
        await rphProcess.SendQuickLogInfoMessage(context);
    }
    private async Task ELSThread(ContextMenuContext context, DiscordAttachment attachmentForAnalysis)
    {
        await context.DeferAsync(true);
        ELSProcess elsProcess = new ELSProcess();
        elsProcess.log = ELSAnalyzer.Run(attachmentForAnalysis.Url);
        elsProcess.log.MsgId = context.TargetMessage.Id;
        Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, elsProcess));
        await elsProcess.SendQuickLogInfoMessage(context);
    }
    private async Task ASIThread(ContextMenuContext context, DiscordAttachment attachmentForAnalysis)
    {
        await context.DeferAsync(true);
        ASIProcess asiProcess = new ASIProcess();
        asiProcess.log = ASIAnalyzer.Run(attachmentForAnalysis.Url);
        asiProcess.log.MsgId = context.TargetMessage.Id;
        Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, asiProcess));
        await asiProcess.SendQuickLogInfoMessage(context);
    }
    private async Task SHVDNThread(ContextMenuContext context, DiscordAttachment attachmentForAnalysis)
    {
        await context.DeferAsync(true);
        SHVDNProcess shvdnProcess = new SHVDNProcess();
        shvdnProcess.log = SHVDNAnalyzer.Run(attachmentForAnalysis.Url);
        shvdnProcess.log.MsgId = context.TargetMessage.Id;
        Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, shvdnProcess));
        await shvdnProcess.SendQuickLogInfoMessage(context);
    }
}