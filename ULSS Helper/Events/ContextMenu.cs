using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.ELS_Modules;
using ULSS_Helper.Modules.RPH_Modules;

namespace ULSS_Helper.Events;

internal class ContextMenu : ApplicationCommandModule
{
    private string? _file;
    
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
        _file = null;
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
                    var emb = new DiscordInteractionResponseBuilder();
                    emb.IsEphemeral = true;
                    emb.AddEmbed(BasicEmbeds.Error($"No attachment found. There needs to be a {acceptedFileNamesString} log file attached to the message!"));
                    await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                    return;
                case 1:
                    _file = e.TargetMessage.Attachments[0]?.Url;
                    break;
                case > 1:
                    foreach(DiscordAttachment attachment in e.TargetMessage.Attachments)
                    {
                        if (acceptedFileNames.Any(attachment.Url.Contains))
                        {
                            _file = attachment.Url;
                            break;
                        }
                    }
                    if (_file == null)
                    {
                        var emb2 = new DiscordInteractionResponseBuilder();
                        emb2.IsEphemeral = true;
                        emb2.AddEmbed(BasicEmbeds.Error($"There is no log file named {acceptedLogFileNamesString} file attached!"));
                        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb2);
                        return;
                    }
                    break;
            }
            if (_file == null)
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(BasicEmbeds.Error("Failed to load attached file!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            if (!acceptedFileNames.Any(_file.Contains))
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(BasicEmbeds.Error($"This file is not named {acceptedLogFileNamesString}!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (_file.Contains("RagePluginHook"))
            {
                await e.DeferAsync(true);
                RPHProcess rphProcess = new RPHProcess();
                rphProcess.log = RPHAnalyzer.Run(_file);
                rphProcess.log.MsgId = e.TargetMessage.Id;
                Program.Cache.SaveProcess(e.TargetMessage.Id, new(e.Interaction, e.TargetMessage, rphProcess));
                await rphProcess.SendQuickLogInfoMessage(e);
                return;
            }
            if (_file.Contains("ELS"))
            {
                await e.DeferAsync(true);
                ELSProcess elsProcess = new ELSProcess();
                elsProcess.log = ELSAnalyzer.Run(_file);
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