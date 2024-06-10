using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Modules.XML_Modules;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;
using ULSS_Helper.Public.AutoHelper.Modules.Process_Files;
using RPHProcess = ULSS_Helper.Public.AutoHelper.Modules.Process_Files.RPHProcess;

namespace ULSS_Helper.Public.AutoHelper;

public class MessageSent
{
    internal static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        try
        {
            if (ctx.Message.MessageType == DiscordMessageType.ThreadCreated && ctx.Message.ChannelId == Program.Settings.Env.AutoHelperChannelId) 
                await ctx.Channel.DeleteMessageAsync(ctx.Message);
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
                        1 => 12,
                        0 => 6,
                        _ => ac.Timer
                    };
                    await Database.EditCase(ac);

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
                    var dltMsg = false;
                    foreach (var attach in ctx.Message.Attachments)
                    {
                        var rs = $">>> User: {ctx.Author.Mention} ({ctx.Author.Id.ToString()})\r\nLog: {ctx.Message.JumpLink}\r\n" +
                                 $"User sent a log greater than 3MB!\r\nFile Size: {attach.FileSize/1000000}MB";
                        var os = "__AutoBlacklisted!__\r\nYou have sent a log bigger than 3MB! You may not use the AutoHelper until staff review this!";
                        switch (attach.FileName)
                        {
                            case "RagePluginHook.log":
                                if (attach.FileSize / 1000000 > 3)
                                {
                                    await ctx.Message.RespondAsync(BasicEmbeds.Error(os, true));
                                    AutoBlacklist.Add(ctx.Author.Id.ToString(), rs);
                                    return;
                                }
                                await RPHProcess.ProcessLog(attach, ctx);
                                break;
                            case "ELS.log":
                                if (attach.FileSize / 1000000 > 3)
                                {
                                    await ctx.Message.RespondAsync(BasicEmbeds.Error(os, true));
                                    AutoBlacklist.Add(ctx.Author.Id.ToString(), rs);
                                    return;
                                }
                                await ELSProcess.ProcessLog(attach, ctx);
                                break;
                            case "asiloader.log":
                                if (attach.FileSize / 1000000 > 3)
                                {
                                    await ctx.Message.RespondAsync(BasicEmbeds.Error(os, true));
                                    AutoBlacklist.Add(ctx.Author.Id.ToString(), rs);
                                    return;
                                }
                                await ASIProcess.ProcessLog(attach, ctx);
                                break;
                            case "ScriptHookVDotNet.log":
                                if (attach.FileSize / 1000000 > 3)
                                {
                                    await ctx.Message.RespondAsync(BasicEmbeds.Error(os, true));
                                    AutoBlacklist.Add(ctx.Author.Id.ToString(), rs);
                                    return;
                                }
                                await SHVDNProcess.ProcessLog(attach, ctx);
                                break;
                            default:
                                if (attach.FileName.EndsWith(".xml") || attach.FileName.EndsWith(".meta"))
                                {
                                    if (attach.FileSize / 1000000 > 5)
                                    {
                                        await ctx.Message.RespondAsync(BasicEmbeds.Error(os, true));
                                        AutoBlacklist.Add(ctx.Author.Id.ToString(), rs);
                                        return;
                                    }
                                    var xmlMsg = await XMLValidator.Run(attach.Url);
                                    await ctx.Message.RespondAsync(BasicEmbeds.Public(
                                        $"## __ULSS AutoHelper__\r\n```{xmlMsg}```"));
                                }
                                // if (attach.FileName.EndsWith(".png") || attach.FileName.EndsWith(".jpg"))
                                //      await ImageProcess.ProcessImage(attach, ctx);
                                if (attach.FileName.EndsWith(".log") || attach.FileName.EndsWith(".txt" ) || attach.FileName.EndsWith(".rcr" ))
                                    await ctx.Message.RespondAsync(BasicEmbeds.Public(
                                        "## __ULSS AutoHelper__\r\nThis file is not supported or is not named correctly!")); 
                                if (attach.FileName.EndsWith(".exe") || attach.FileName.EndsWith(".dll") || attach.FileName.EndsWith(".asi"))
                                {
                                    await ctx.Message.RespondAsync(BasicEmbeds.Error($"__ULSS AutoHelper__\r\n>>> Do not upload executable files!\r\nFile: {attach.FileName}", true));
                                    dltMsg = true;
                                }
                                break;
                        }
                    }
                    if (dltMsg) await ctx.Message.DeleteAsync();
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