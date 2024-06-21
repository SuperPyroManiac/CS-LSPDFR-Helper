using DSharpPlus.Entities;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions;

public class Startup
{
    private static int _addedCnt;
    private static int _changedCnt;
    
    public static async Task Init()
    {
        await Verification();
        await PrepCaches();
        await SendStartupMessage();
    }
    
    private static async Task Verification()
    {
        //User Verifications
        _addedCnt = await Verifications.Users.Missing();
        _changedCnt = await Verifications.Users.Usernames();
    }

    private static Task PrepCaches()
    {
        Program.Cache.UpdateUsers(DbManager.GetUsers());
        Program.Cache.UpdateErrors(DbManager.GetErrors());
        Program.Cache.UpdatePlugins(DbManager.GetPlugins());
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
                   $"> **Cached users:** {Program.Cache.GetUsers().Count}";
        if (_addedCnt > 0)
            msgText += $"> {_addedCnt} New users found, added them to the DB!\r\n";
        if (_changedCnt > 0)
            msgText += $"> {_changedCnt} Username changes, updated the DB!\r\n";
        
        var embed = BasicEmbeds.Success(msgText, true);
        var ch = await Functions.GetGuild().GetChannelAsync(Program.Settings.BotLogChId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(ch);
    }
}