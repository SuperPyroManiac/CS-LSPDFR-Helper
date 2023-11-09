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
        try
        {
            //===//===//===////===//===//===////===//Attachment Checks/===////===//===//===////===//===//===//
            switch (e.TargetMessage.Attachments.Count)
            {
                case 0:
                    var emb = new DiscordInteractionResponseBuilder();
                    emb.IsEphemeral = true;
                    emb.AddEmbed(BasicEmbeds.Error("No attachment found. There needs to be a RPH or ELS log file attached to the message!"));
                    await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                    return;
                case 1:
                    _file = e.TargetMessage.Attachments[0]?.Url;
                    break;
                case > 1:
                    foreach(DiscordAttachment attachment in e.TargetMessage.Attachments)
                    {
                        if (attachment.Url.Contains("RagePluginHook.log"))
                        {
                            _file = attachment.Url;
                            break;
                        }
                    }
                    if (_file == null)
                    {
                        var emb2 = new DiscordInteractionResponseBuilder();
                        emb2.IsEphemeral = true;
                        emb2.AddEmbed(BasicEmbeds.Error("There is no file named `RagePluginHook.log!`"));
                        await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb2);
                        return;
                    }
                    break;
            }
            if (_file == null)
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(BasicEmbeds.Error("Failed to load `RagePluginHook.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            if (!_file.Contains("RagePluginHook") && !_file.Contains("ELS"))
            {
                var emb = new DiscordInteractionResponseBuilder();
                emb.IsEphemeral = true;
                emb.AddEmbed(BasicEmbeds.Error("This file is not named `RagePluginHook.log` or `ELS.log`!"));
                await e.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, emb);
                return;
            }
            //===//===//===////===//===//===////===//Process Attachments/===////===//===//===////===//===//===//
            if (_file.Contains("RagePluginHook"))
            {
                await e.DeferAsync(true);
                RPHProcess.log = RPHAnalyzer.Run(_file);
                RPHProcess.log.MsgId = e.TargetMessage.Id;
                await RPHProcess.SendQuickLogInfoMessage(e);
                return;
            }
            if (_file.Contains("ELS"))
            {
                await e.DeferAsync(true);
                ELSProcess.log = ELSAnalyzer.Run(_file);
                ELSProcess.log.MsgId = e.TargetMessage.Id;
                await ELSProcess.SendQuickLogInfoMessage(e);
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