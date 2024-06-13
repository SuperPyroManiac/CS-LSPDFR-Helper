using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class StatusMessages
{
    internal static async Task SendStartupMessage()
    {
        var branchName = "";
        var commitHash = "";
        var commitHashShort = "";
        var addedCnt = 0;
        try
        { 
            var infoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "build_info.txt");
            if (File.Exists(infoFilePath)) 
            {
                var buildInfo = await File.ReadAllTextAsync(infoFilePath);
                branchName = buildInfo.Split("Current Branch: ")[1].Split("|")[0].Trim();
                commitHash = buildInfo.Split("Commit Hash: ")[1].Trim();
                commitHashShort = commitHash.Substring(0, 7);
            }

            var serverusers = Program.GetGuild().Members.Values.ToList();
            var dbusers = Database.LoadUsers();

            foreach (var user in serverusers)
            {
                if (dbusers.All(x => x.UID.ToString() != user.Id.ToString()))
                {
                    if (user == null) continue;
                    addedCnt++;

                    var newUser = new ULSS_Helper.Objects.DiscordUser()
                    {
                        UID = user.Id.ToString(),
                        Username = user.Username,
                        BotEditor = 0,
                        BotAdmin = 0,
                        Blocked = 0
                    };
                    Database.AddUser(newUser);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog(e.ToString());
        }
        var msgText = "__Tara Helper woke up from her beauty sleep!__\n\n";
        if (!string.IsNullOrEmpty(commitHash))
            msgText += $"> Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash}) (branch: `{branchName}`)\r\n";
        if (addedCnt > 0)
            msgText += $"> {addedCnt} New users found, added them to the DB!";
        
        var embed = BasicEmbeds.Success(msgText, true);
        var tmplogchnl = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(tmplogchnl);
    }
}