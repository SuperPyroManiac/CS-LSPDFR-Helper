using DSharpPlus.Entities;
using LSPDFR_Helper.Functions.Messages;

namespace LSPDFR_Helper.Functions;

internal class Startup
{
    private static int _addedCnt;
    private static int _changedCnt;
    
    internal static async Task Init()
    {
        PrepCaches();
        await Verification();
        await SendStartupMessage();
    }

    private static void PrepCaches()
    {
        Program.Cache.UpdateUsers(DbManager.GetUsers());
    }

    private static async Task Verification()
    {
        //User Verifications
        _addedCnt = await Verifications.Users.Missing();
        _changedCnt = await Verifications.Users.Usernames();
    }

    private static async Task SendStartupMessage()
    {
        const string branchName = "";
        const string commitHash = "";
        const string commitHashShort = "";
        
        var msgText = "__Tara Helper woke up from her beauty sleep!__\n\n";
        if (!string.IsNullOrEmpty(commitHash))
            msgText += $"> Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash}) (branch: `{branchName}`)\r\n";
        if (_addedCnt > 0)
            msgText += $"> {_addedCnt} New users found, added them to the DB!\r\n";
        if (_changedCnt > 0)
            msgText += $"> {_changedCnt} Username changes, updated the DB!\r\n";
        
        var embed = BasicEmbeds.Success(msgText, true);
        var ch = await Functions.GetGuild().GetChannelAsync(Program.Settings.BotLogChId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(ch);
    }
}