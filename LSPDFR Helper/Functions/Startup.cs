using DSharpPlus.Entities;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Functions;

public class Startup
{
    private static int _addedUsrCnt;
    private static int _changedUsrCnt;
    
    public static async Task Init()
    {
        await Task.WhenAll(Verification(), PrepCaches());
        await SendStartupMessage();
    }
    
    private static async Task Verification()
    {
        //User Verifications
        _addedUsrCnt = await Users.Missing();
        _changedUsrCnt = await Users.Usernames();
        
        //AH Verifications
        await Verifications.AutoHelper.UpdateMainAhMessage();
        await Verifications.AutoHelper.UpdateAhMonitor();
    }

    private static Task PrepCaches()
    {
        Program.Cache.UpdateUsers(DbManager.GetUsers());
        Program.Cache.UpdateErrors(DbManager.GetErrors());
        Program.Cache.UpdatePlugins(DbManager.GetPlugins());
        Program.Cache.UpdateCases(DbManager.GetCases());
        return Task.CompletedTask;
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

        
        var msgText = "__Tara Helper woke up from her beauty sleep!__\n\n";
        if ( !string.IsNullOrEmpty(commitHash) )
            msgText += $"> Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash})\r\n";
        msgText += $"> **Cached plugins:** {Program.Cache.GetPlugins().Count}\r\n" +
                   $"> **Cached errors:** {Program.Cache.GetErrors().Count}\r\n" +
                   $"> **Cached cases:** {Program.Cache.GetCases().Count}\r\n" +
                   $"> **Cached users:** {Program.Cache.GetUsers().Count}\r\n\r\n";
        if (_addedUsrCnt > 0)
            msgText += $"> *{_addedUsrCnt} New users found, added them to the DB!*\r\n";
        if (_changedUsrCnt > 0)
            msgText += $"> *{_changedUsrCnt} Username changes, updated the DB!*\r\n";
        
        var embed = BasicEmbeds.Success(msgText);
        var ch = await Functions.GetGuild().GetChannelAsync(Program.Settings.BotLogChId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(ch);
    }
}