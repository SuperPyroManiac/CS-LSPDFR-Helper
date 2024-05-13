using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.XML_Modules;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;
using ULSS_Helper.Public.AutoHelper.Modules.Process_Files;
using RPHProcess = ULSS_Helper.Public.AutoHelper.Modules.Process_Files.RPHProcess;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreateEventArgs ctx)
    {
        try
        {
            if (ctx.Channel.IsPrivate) return;
            if (ctx.Message.MessageType == MessageType.ThreadCreated) await ctx.Channel.DeleteMessageAsync(ctx.Message);
            if (Program.Cache.GetCasess().Any(x => x.ChannelID == ctx.Channel.Id.ToString()) && !ctx.Author.IsBot)
            {
                var ac = Program.Cache.GetCasess().First(x => x.ChannelID == ctx.Channel.Id.ToString());

                if (Program.Cache.GetUser(ac.OwnerID).Blocked == 1)
                {
                    await ctx.Message.RespondAsync(BasicEmbeds.Error(
                        $"__You are blacklisted from the bot!__\r\nContact server staff in <#{Program.Settings.Env.StaffContactChannelId}> if you think this is an error!", true));
                    await CloseCase.Close(ac);
                    return;
                }
                
                if (ac.OwnerID == ctx.Author.Id.ToString())
                {
                    ac.Timer = ac.TsRequested switch
                    {
                        1 => 24,
                        0 => 6,
                        _ => ac.Timer
                    };
                    Database.EditCase(ac);

                    foreach (var error in Program.Cache.GetErrors().Where(error => error.Level == "PMSG"))
                    {
                        var errregex = new Regex(error.Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        var errmatch = errregex.Match(ctx.Message.Content);
                        if (errmatch.Success)
                        {
                            var emb = BasicEmbeds.Public(
                                $"## __ULSS AutoHelper__\r\n>>> {error.Solution}");
                            emb.Footer.Text = emb.Footer.Text + $" - ID: {error.ID}";
                            await ctx.Message.RespondAsync(emb);
                        }
                    }
                    
                    if (ctx.Message.Attachments.Count == 0) return;
                    foreach (var attach in ctx.Message.Attachments)
                    {
                        switch (attach.FileName)
                        {
                            case "RagePluginHook.log":
                                await RPHProcess.ProcessLog(attach, ctx);
                                break;
                            case "ELS.log":
                                await ELSProcess.ProcessLog(attach, ctx);
                                break;
                            case "asiloader.log":
                                await ASIProcess.ProcessLog(attach, ctx);
                                break;
                            case "ScriptHookVDotNet.log":
                                await SHVDNProcess.ProcessLog(attach, ctx);
                                break;
                            default:
                                if (attach.FileName.EndsWith(".xml") || attach.FileName.EndsWith(".meta"))
                                {
                                    var xmlMsg = await XMLValidator.Run(attach.Url);
                                    await ctx.Message.RespondAsync(BasicEmbeds.Public(
                                        $"## __ULSS AutoHelper__\r\n```{xmlMsg}```"));
                                }
                                // if (attach.FileName.EndsWith(".png") || attach.FileName.EndsWith(".jpg"))
                                //      await ImageProcess.ProcessImage(attach, ctx);
                                if (attach.FileName.EndsWith(".log") || attach.FileName.EndsWith(".txt" ) || attach.FileName.EndsWith(".rcr" ))
                                    await ctx.Message.RespondAsync(BasicEmbeds.Public(
                                        "## __ULSS AutoHelper__\r\nThis file is not supported or is not named correctly!"));
                                break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            await Logging.ErrLog(e.ToString());
            Console.WriteLine(e);
            throw;
        }
    }
}