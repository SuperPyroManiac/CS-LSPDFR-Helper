using DSharpPlus.Entities;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CaseMonitor
{
    internal static async Task UpdateMonitor()
    {
        var cl = Program.Client;
        var ch = await cl.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId);
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

        var allCases = Database.LoadCases().Where(ac => ac.Solved == 0).ToList().OrderBy(ac => ac.Timer);
        foreach (var ac in allCases.TakeWhile(ac => embed.Fields.Count < 16))
        {
            if (embed.Fields.Count == 15)
            {
                embed.AddField("..And More", "There are too many cases to show!");
                break;
            }

            var tmpclg = await cl.GetGuildAsync(Program.Settings.Env.ServerId);
            var tmpclc = await cl.GetChannelAsync(ulong.Parse(ac.ChannelID));
            var tmpusr = await tmpclg.GetMemberAsync(ulong.Parse(ac.OwnerID));
            
            if (ac.Solved == 0)
                embed.AddField($"__<#{ac.ChannelID}>__",
                    $">>> Author: {tmpusr.DisplayName}"
                    + $"\r\nHelp Requested: {Convert.ToBoolean(ac.TsRequested)}"
                    + $"\r\nCreated: <t:{tmpclc.CreationTimestamp.ToUnixTimeSeconds()}:R> | AutoClose: `{ac.Timer}` hours");
        }
        if (embed.Fields.Count == 0) embed.AddField("None", "No open cases!");

        await new DiscordMessageBuilder().AddEmbed(embed).ModifyAsync(origMsg);
    }
}