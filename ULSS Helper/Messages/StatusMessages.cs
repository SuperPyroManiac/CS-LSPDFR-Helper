using DSharpPlus.Entities;

namespace ULSS_Helper.Messages;

internal class StatusMessages
{
    internal static void SendStartupMessage()
    {
        string branchName = "";
        string commitHash = "";
        string commitHashShort = "";
        try 
        {
            string? infoFilePath = Path.Combine(Directory.GetCurrentDirectory(), "build_info.txt");
            if (File.Exists(infoFilePath)) 
            {
                string buildInfo = File.ReadAllText(infoFilePath);
                branchName = buildInfo.Split("Current Branch: ")[1].Split("|")[0].Trim();
                commitHash = buildInfo.Split("Commit Hash: ")[1].Trim();
                commitHashShort = commitHash.Substring(0, 7);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Logging.ErrLog(e.ToString());
        }
        string msgText = "**Tara Helper woke up from her beauty sleep!**\n\n";
        if (!string.IsNullOrEmpty(commitHash))
            msgText += $"Build is based on commit with hash [`{commitHashShort}`](https://github.com/SuperPyroManiac/ULSS-Helper/commit/{commitHash}) (branch: `{branchName}`)\n";
        
        DiscordEmbedBuilder embed = BasicEmbeds.Success(msgText);
        var msg = new DiscordMessageBuilder()
            .WithEmbed(embed)
            .SendAsync(Program.Client.GetChannelAsync(Program.Settings.Env.TsBotLogChannelId).Result);
    }
}