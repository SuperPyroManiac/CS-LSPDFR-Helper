using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Events;

internal class ContextMenu : ApplicationCommandModule
{
    private static DiscordAttachment? attachmentForAnalysis;
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Analyze Log")]
    public async Task OnMenuSelect(ContextMenuContext e)
    {
        //===//===//===////===//===//===////===//Permissions/===////===//===//===////===//===//===//
        if (e.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            var emb = new DiscordInteractionResponseBuilder();
            emb.IsEphemeral = true;
            emb.AddEmbed(BasicEmbeds.Error("You do not have permission for this!"));
            await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
            return;
        }

        //===//===//===////===//===//===////===//Attachment Checks/===////===//===//===////===//===//===//
        attachmentForAnalysis = null;
        List<string> acceptedFileNames = new(new string[]{
            "RagePluginHook",
            "ELS"
        });
        string acceptedFileNamesString = string.Join(" or ", acceptedFileNames);
        string acceptedLogFileNamesString = "`" + string.Join(".log` or `", acceptedFileNames) + ".log`";
        try
        {
            switch (e.TargetMessage.Attachments.Count)
            {
                case 0:
                    await LogAnalysisProcess.SendAttachmentErrorMessage(e, $"No attachment found. There needs to be a {acceptedFileNamesString} log file attached to the message!");
                    return;
                case 1:
                    attachmentForAnalysis = e.TargetMessage.Attachments[0];
                    break;
                case > 1:
                    List<DiscordAttachment> acceptedAttachments = new List<DiscordAttachment>();
                    foreach(DiscordAttachment attachment in e.TargetMessage.Attachments)
                    {
                        if (acceptedFileNames.Any(attachment.FileName.Contains))
                        {
                            acceptedAttachments.Add(attachment);
                        }
                    }
                    if (acceptedAttachments.Count == 0)
                    {
                        await LogAnalysisProcess.SendAttachmentErrorMessage(e, $"There is no file named {acceptedLogFileNamesString} attached!");
                        return;
                    }
                    else if (acceptedAttachments.Count == 1) 
                        attachmentForAnalysis = acceptedAttachments[0];
                    else if (acceptedAttachments.Count > 1)
                    {
                        await e.DeferAsync(true);
                        await LogAnalysisProcess.SendSelectFileForAnalysisMessage(e, acceptedAttachments);
                        return;
                    }
                    break;
            }
            
            if (attachmentForAnalysis == null)
            {
                await LogAnalysisProcess.SendAttachmentErrorMessage(e, "Failed to load attached file!");
                return;
            }
            if (!acceptedFileNames.Any(attachmentForAnalysis.FileName.Contains))
            {
                await LogAnalysisProcess.SendAttachmentErrorMessage(e, $"This file is not named {acceptedLogFileNamesString}!");
                return;
            }
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (attachmentForAnalysis.FileName.Contains("RagePluginHook"))
            {
                await e.DeferAsync(true);
                RPHProcess rphProcess = new RPHProcess();
                rphProcess.log = RPHAnalyzer.Run(attachmentForAnalysis.Url);
                rphProcess.log.MsgId = e.TargetMessage.Id;
                Program.Cache.SaveProcess(e.TargetMessage.Id, new(e.Interaction, e.TargetMessage, rphProcess));
                await rphProcess.SendQuickLogInfoMessage(e);
                return;
            }
            if (attachmentForAnalysis.FileName.Contains("ELS"))
            {
                await e.DeferAsync(true);
                ELSProcess elsProcess = new ELSProcess();
                elsProcess.log = ELSAnalyzer.Run(attachmentForAnalysis.Url);
                elsProcess.log.MsgId = e.TargetMessage.Id;
                Program.Cache.SaveProcess(e.TargetMessage.Id, new(e.Interaction, e.TargetMessage, elsProcess));
                await elsProcess.SendQuickLogInfoMessage(e);
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