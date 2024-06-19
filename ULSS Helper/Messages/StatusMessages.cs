using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

public class StatusMessages
{
    public static async Task SendStartupMessage()
    {
        var branchName = "";
        var commitHash = "";
        var commitHashShort = "";
        var addedCnt = 0;
        var changedCnt = 0;
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

            var serverusers = Program.GetGuild().Members;
            var dbusers = Database.LoadUsers();

            foreach (var user in serverusers.Values.ToList())
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

            foreach (var user in dbusers)
            {
                if (!serverusers.ContainsKey(ulong.Parse(user.UID))) continue;
                if (serverusers[ulong.Parse(user.UID)].Username != user.Username)
                {
                    changedCnt++;
                    user.Username = serverusers[ulong.Parse(user.UID)].Username;
                    await Task.Delay(100);
                    Database.EditUser(user);
                }
            }
            if (addedCnt > 0 || changedCnt > 0) Program.Cache.UpdateUsers(Database.LoadUsers());
            
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
            msgText += $"> {addedCnt} New users found, added them to the DB!\r\n";
        if (changedCnt > 0)
            msgText += $"> {changedCnt} Username changes, updated the DB!\r\n";
        
        var embed = BasicEmbeds.Success(msgText, true);
        var tmplogchnl = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(tmplogchnl);
    }
}