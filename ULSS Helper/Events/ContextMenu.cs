using DSharpPlus;
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
    private static DiscordAttachment? attachmentForAnalysis;
    
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
        attachmentForAnalysis = null;
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
                    attachmentForAnalysis = context.TargetMessage.Attachments[0];
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
                        attachmentForAnalysis = acceptedAttachments[0];
                    else if (acceptedAttachments.Count > 1)
                    {
                        await context.DeferAsync(true);
                        await sharedLogInfo.SendSelectFileForAnalysisMessage(context, acceptedAttachments);
                        return;
                    }
                    break;
            }
            
            if (attachmentForAnalysis == null)
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, "Failed to load attached file!");
                return;
            }
            if (!acceptedFileNames.Any(attachmentForAnalysis.FileName.Contains))
            {
                await sharedLogInfo.SendAttachmentErrorMessage(context, $"This file is not named {acceptedLogFileNamesString}!");
                return;
            }
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (attachmentForAnalysis.FileName.Contains("RagePluginHook"))
            {
                await context.DeferAsync(true);
                RPHProcess rphProcess = new RPHProcess();
                rphProcess.log = RPHAnalyzer.Run(attachmentForAnalysis.Url);
                rphProcess.log.MsgId = context.TargetMessage.Id;
                rphProcess.log.DownloadLink = attachmentForAnalysis.Url;
                Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, rphProcess));
                await rphProcess.SendQuickLogInfoMessage(context);
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("ELS"))
            {
                await context.DeferAsync(true);
                ELSProcess elsProcess = new ELSProcess();
                elsProcess.log = ELSAnalyzer.Run(attachmentForAnalysis.Url);
                elsProcess.log.MsgId = context.TargetMessage.Id;
                elsProcess.log.DownloadLink = attachmentForAnalysis.Url;
                Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, elsProcess));
                await elsProcess.SendQuickLogInfoMessage(context);
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("asiloader"))
            {
                await context.DeferAsync(true);
                ASIProcess asiProcess = new ASIProcess();
                asiProcess.log = ASIAnalyzer.Run(attachmentForAnalysis.Url);
                asiProcess.log.MsgId = context.TargetMessage.Id;
                asiProcess.log.DownloadLink = attachmentForAnalysis.Url;
                Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, asiProcess));
                await asiProcess.SendQuickLogInfoMessage(context);
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("ScriptHookVDotNet"))
            {
                await context.DeferAsync(true);
                SHVDNProcess shvdnProcess = new SHVDNProcess();
                shvdnProcess.log = SHVDNAnalyzer.Run(attachmentForAnalysis.Url);
                shvdnProcess.log.MsgId = context.TargetMessage.Id;
                shvdnProcess.log.DownloadLink = attachmentForAnalysis.Url;
                Program.Cache.SaveProcess(context.TargetMessage.Id, new(context.Interaction, context.TargetMessage, shvdnProcess));
                await shvdnProcess.SendQuickLogInfoMessage(context);
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
}