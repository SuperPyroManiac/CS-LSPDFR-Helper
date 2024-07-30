using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Functions;

public static class Startup
{
    private static int _addedSrvCnt;
    private static int _removedSrvCnt;
    private static int _addedUsrCnt;
    private static int _changedUsrCnt;
    private static int _closedCaseCnt;
    
    public static async Task Init()
    {
        await PrepCaches();
        await Verification();
        await SendStartupMessage();
    }
    
    private static Task PrepCaches()
    {
        Program.Cache.UpdateUsers(DbManager.GetUsers());
        Program.Cache.UpdateErrors(DbManager.GetErrors());
        Program.Cache.UpdatePlugins(DbManager.GetPlugins());
        Program.Cache.UpdateCases(DbManager.GetCases());
        Program.Cache.UpdateServers(DbManager.GetServers());
        return Task.CompletedTask;
    }
    
    private static async Task Verification()
    {
        //Server Verifications
        _addedSrvCnt = await Servers.AddMissing();
        _removedSrvCnt = await Servers.RemoveMissing();
        await Servers.Validate();
        
        //User Verifications
        _addedUsrCnt = await Users.Missing();
        _changedUsrCnt = await Users.Usernames();
        
        //AH Verifications
        _closedCaseCnt = await Verifications.AutoHelper.ValidateClosedCases();
        _closedCaseCnt += await Verifications.AutoHelper.ValidateOpenCases();
        await Verifications.AutoHelper.UpdateMainAhMessage();
        await Verifications.AutoHelper.UpdateAhMonitor();
    }

    private static async Task SendStartupMessage()
    {
        var commitHash = "";
        var commitHashShort = "";
        var infoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "build_info.txt");
        if (File.Exists(infoFilePath)) 
        {
            var buildInfo = await File.ReadAllTextAsync(infoFilePath);
            commitHash = buildInfo.Split("Commit Hash: ")[1].Trim();
            commitHashShort = commitHash[..7];
        }

        
        var msgText = "__LSPDFR Helper Has Loaded!__\n\n";
        if ( !string.IsNullOrEmpty(commitHash) )
            msgText += $"> Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash})\r\n";
        msgText += $"> **Cached servers:** {Program.Cache.ServerCacheDict.Count}\r\n" +
                   $"> **Cached plugins:** {Program.Cache.GetPlugins().Count}\r\n" +
                   $"> **Cached errors:** {Program.Cache.GetErrors().Count}\r\n" +
                   $"> **Cached cases:** {Program.Cache.GetCases().Count}\r\n" +
                   $"> **Cached users:** {Program.Cache.GetUsers().Count}\r\n\r\n";
        if (_addedSrvCnt > 0) msgText += $"> *{_addedSrvCnt} New servers found, added them to the DB!*\r\n";
        if (_removedSrvCnt > 0) msgText += $"> *{_removedSrvCnt} Servers not found, removed them from the DB!*\r\n";
        if (_addedUsrCnt > 0) msgText += $"> *{_addedUsrCnt} New users found, added them to the DB!*\r\n";
        if (_changedUsrCnt > 0) msgText += $"> *{_changedUsrCnt} Username changes, updated the DB!*\r\n";
        if (_closedCaseCnt > 0) msgText += $"> *{_closedCaseCnt} Cases failed verification, closed!*\r\n";
        
        var embed = BasicEmbeds.Success(msgText);
        var ch = await Program.BotSettings.BasicLogs();
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(ch);
    }
}