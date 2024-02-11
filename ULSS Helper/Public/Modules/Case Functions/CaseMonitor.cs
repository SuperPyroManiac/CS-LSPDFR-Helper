using DSharpPlus.Entities;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.Modules.Case_Functions;

internal class CaseMonitor
{
    internal static async Task UpdateMonitor()
    {
        var cl = Program.Client;
        var ch = cl.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId).Result;
        List<DiscordMessage> msgPurge = [];
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Public("# __AutoHelper Active Cases__");
        await foreach (var msg in ch.GetMessagesAsync(100))
        {
            if (msg.Embeds.Count == 0) msgPurge.Add(msg);
            foreach (var emb in msg.Embeds)
            {
                if (!emb.Description.Contains("AutoHelper Active Cases") &&
                    !emb.Description.Contains("Help Requested!"))
                {
                    msgPurge.Add(msg);
                }
                if (emb.Description.Contains("AutoHelper Active Cases"))
                {
                    origMsg = msg;
                }
            }
        }
        foreach (var msg in msgPurge)
        {
            await ch.DeleteMessageAsync(msg);
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");
        
        foreach (var ac in Database.LoadCases().TakeWhile(ac => embed.Fields.Count < 25))
        {
            if (embed.Fields.Count == 10)
            {
                embed.AddField("..And More", "There are too many cases to show!");
                break;
            }
            
            if (ac.Solved == 0)
                embed.AddField($"__<#{ac.ChannelID}>__",
                    $">>> Author: {cl.GetGuildAsync(Program.Settings.Env.ServerId).Result.GetMemberAsync(ulong.Parse(ac.OwnerID)).Result.DisplayName}"
                    + $"\r\nHelp Requested: {Convert.ToBoolean(ac.TsRequested)}"
                    + $"\r\nCreated: <t:{cl.GetChannelAsync(ulong.Parse(ac.ChannelID)).Result.CreationTimestamp.ToUnixTimeSeconds()}:R> | AutoClose: `{ac.Timer}` hours");
        }
        if (embed.Fields.Count == 0) embed.AddField("None", "No open cases!");

        await OpenCase.UpdateMsg();
        await new DiscordMessageBuilder().AddEmbed(embed).ModifyAsync(origMsg);
    }
}