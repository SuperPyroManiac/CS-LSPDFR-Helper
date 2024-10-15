using DSharpPlus;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.MainTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.AutoHelper;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Functions.Verifications;

public class AutoHelper
{
    public static async Task UpdateMainAhMessage(ulong serverId)
    {
        try
        {
            var description = 
                //"\r\n> The AutoHelper can read a variety of file types and will attempt to find issues. Currently supported log files are **RagePluginHook**, **ELS**, and **ASI** logs. The AutoHelper can also parse **.xml** and **.meta** files as well as **.png** and **.jpg** images!"+
                "\r\n> The AutoHelper can read a variety of file types and will attempt to find issues. Currently supported log files are **RagePluginHook**, **ELS**, and **ASI** logs. The AutoHelper can also parse **.xml** and **.meta** files as well."+
                "\r\n> Please note that frequent issues can often be detected, but human assistance may be required for more advanced problems. you may wish to use the request button to ask for human help." +
                "\r\n\r\n## __AutoHelper Terms Of Use__" +
                "\r\n> - Do not send modified logs to 'test' the bot. Access will instantly be revoked." +
                "\r\n> - Do not upload logs or files greater than **__3MB__**. Access will instantly be revoked." +
                "\r\n> - Do not spam cases. You can upload multiple logs to a single case." +
                "\r\n> - Proxy support is frowned upon. Ideally you should have the user upload their log directly." +
                "\r\n\r\n## __Other Info__" +
                "\r\n> Anyone can join and assist in cases, using /JoinCase to do so. You can request help from others using the button, we ask that you do not abuse this feature though." + 
                "\r\n\r\n> __Managed by: SuperPyroManiac & Hammer__\r\n> More information at: https://dsc.PyrosFun.com";
            
            var server = Program.Cache.GetServer(serverId);
            if ( server.AutoHelperChId == 0 ) return;
            var chh = await Program.Client.GetChannelAsync(server.AutoHelperChId);
            var st = DbManager.AutoHelperStatus(serverId);
            DiscordMessage origMsgg = null;
            var embedd = BasicEmbeds.Ts("# __LSPDFR AutoHelper__", null);
            await foreach (var msg in chh.GetMessagesAsync())
            {
                if (msg.Embeds.Count <= 0) continue;
                var first = msg.Embeds.FirstOrDefault();
                if (first!.Description != null && msg.Embeds.FirstOrDefault()!.Description!.Contains("LSPDFR AutoHelper")) origMsgg = msg;
            }
            if (origMsgg == null) origMsgg = await chh.SendMessageAsync("Starting...");

            embedd.Description += description;
            if (!st) embedd.Description += "\r\n\r\n## __AutoHelper Disabled!__\r\n>>> **System has been disabled by staff temporarily!**";
        
            await new DiscordMessageBuilder()
                .AddEmbed(embedd)
                .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Success, CustomIds.OpenCase, "Open Case", !st))
                .ModifyAsync(origMsgg);
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
    
    public static async Task UpdateAhMonitor(ulong serverId)
    {
        try
        {
            var server = Program.Cache.GetServer(serverId);
            if ( server.MonitorChId == 0 ) return;
            var ch = await Program.Client.GetChannelAsync(server.MonitorChId);
            DiscordMessage origMsg = null;
            var embed = BasicEmbeds.Ts("# __AutoHelper Active Cases__", null);
            await foreach (var msg in ch.GetMessagesAsync())
            {
                if (msg.Author!.Id != 1189354194205950072 && msg.Author!.Id != 1140824901104701440 && msg.Author!.Id != 1268392593243373577) continue;
                if (msg.Embeds.Count == 0) continue;
                foreach (var emb in msg.Embeds)
                {
                    if (emb.Description!.Contains("AutoHelper Active Cases"))
                    {
                        origMsg = msg;
                    }
                }
            }
            if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");

            var allCases = Program.Cache.GetCases().Where(ac => !ac.Solved).ToList().OrderBy(ac => ac.ExpireDate);
            foreach (var ac in allCases.TakeWhile(_ => embed.Fields.Count < 16))
            {
                if (embed.Fields.Count == 15)
                {
                    embed.AddField("..And More", "There are too many cases to show!");
                    break;
                }
            
                if (!ac.Solved && ac.ServerId == server.ServerId)
                    embed.AddField($"__<#{ac.ChannelId}>__",
                        $">>> Author: <@{ac.OwnerId}>"
                        + $"\r\nHelp Requested: {Convert.ToBoolean(ac.TsRequested)}"
                        + $"\r\nCreated: {Formatter.Timestamp(ac.CreateDate.ToLocalTime())} | AutoClose: {Formatter.Timestamp(ac.ExpireDate.ToLocalTime())}");
            }
            if (embed.Fields.Count == 0) embed.AddField("None", "No open cases!");

            await new DiscordMessageBuilder().AddEmbed(embed).ModifyAsync(origMsg);
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
        }
    }
    
    public static async Task<int> ValidateOpenCases()
    {
        try
        {
            var cnt = 0;
        
            foreach ( var ac in Program.Cache.GetCases().Where(x => x.Solved == false) )
            {
                if ( !Program.Cache.GetServer(ac.ServerId).Enabled ) await CloseCase.Close(ac, true);
                if ( ac.ExpireDate <= DateTime.Now.ToUniversalTime() ) { await CloseCase.Close(ac); cnt++; }

                if ( Program.Cache.GetUser(ac.OwnerId).Blocked )
                {
                    var ch = await Program.Client.GetChannelAsync(ac.ChannelId);
                    await ch.SendMessageAsync(BasicEmbeds.Error($"__You are blacklisted from the bot!__\r\n>>> Contact the devs at https://dsc.PyrosFun.com if you think this is an error!"));
                    await CloseCase.Close(ac);
                    cnt++;
                }
            }
            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }
    
    public static async Task<int> ValidateClosedCases()
    {
        try
        {
            var cnt = 0;
            foreach ( var serv in Program.Cache.ServerCacheDict.Values.Where(ac => ac.AutoHelperChId != 0 && ac.Enabled) )
            {
                Dictionary<DiscordThreadChannel, AutoCase> caseChannelDict = new();
    
                var parentCh = await Program.Client.Guilds[serv.ServerId].GetChannelAsync(serv.AutoHelperChId);
                var parentChTh = await parentCh.ListPublicArchivedThreadsAsync(null, 100);
                var thList = parentChTh.Threads.ToList();
                thList.AddRange(parentCh.Threads);
                foreach (var th in thList)
                {
                    if (!th.Name.Contains("AutoHelper")) continue;
                    caseChannelDict.TryAdd(th, Program.Cache.GetCase(th.Name.Split(": ")[1]));
                }

                foreach (var pair in caseChannelDict.Where(c => c.Value != null))
                {
                    if ( pair.Key.ThreadMetadata.IsArchived && pair.Value.Solved ) { await CloseCase.Close(pair.Value); cnt++; }
                    if ( pair.Key.ThreadMetadata.IsArchived == false && pair.Value.Solved ) { await CloseCase.Close(pair.Value); cnt++; }
                }
            }
            return cnt;
        }
        catch ( Exception ex )
        {
            Console.WriteLine(ex);
            await Logging.ErrLog($"{ex}");
            return 0;
        }
    }
}