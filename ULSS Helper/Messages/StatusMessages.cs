using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class StatusMessages
{
    internal static async Task SendStartupMessage()
    {
        var branchName = "";
        var commitHash = "";
        var commitHashShort = "";
        try
        { 
            var infoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "build_info.txt");
            if (File.Exists(infoFilePath)) 
            {
                var buildInfo = File.ReadAllText(infoFilePath);
                branchName = buildInfo.Split("Current Branch: ")[1].Split("|")[0].Trim();
                commitHash = buildInfo.Split("Commit Hash: ")[1].Trim();
                commitHashShort = commitHash.Substring(0, 7);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await Logging.ErrLog(e.ToString());
        }
        var msgText = "**Tara Helper woke up from her beauty sleep!**\n\n";
        if (!string.IsNullOrEmpty(commitHash))
            msgText += $"Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash}) (branch: `{branchName}`)\n";
        
        var embed = BasicEmbeds.Success(msgText);
        var tmplogchnl = await Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId);
        await new DiscordMessageBuilder().AddEmbed(embed).SendAsync(tmplogchnl);
    }
}