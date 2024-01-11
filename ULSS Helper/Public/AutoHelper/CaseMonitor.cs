using DSharpPlus.Entities;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.AutoHelper;

internal class CaseMonitor
{
    internal static async Task UpdateMonitor()
    {
        var cl = Program.Client;
        var ch = cl.GetChannelAsync(Program.Settings.Env.RequestHelpChannelId).Result;
        var origMsg = ch.GetMessageAsync(Program.Settings.Env.ActiveThreadsMessageID).Result; //TODO: Eventually just make a new message at start.
        var embed = BasicEmbeds.Public("# __AutoHelper Active Cases__");

        foreach (var ac in Database.LoadCases().TakeWhile(ac => embed.Fields.Count < 25))
        {
            if (embed.Fields.Count == 24) embed.AddField("..And More", "There are too many cases to show!");
            if (ac.Solved == 0) embed.AddField($"__Case: {ac.CaseID}__", 
                $">>> Author: {cl.GetGuildAsync(Program.Settings.Env.ServerId).Result.GetMemberAsync(ulong.Parse(ac.OwnerID)).Result.DisplayName}" 
                + $"\r\nThread: <#{ac.ChannelID}>"
                + $"\r\nHelp Requested: {Convert.ToBoolean(ac.TsRequested)}"
                + $"\r\nCreated: <t:{cl.GetChannelAsync(ulong.Parse(ac.ChannelID)).Result.CreationTimestamp.ToUnixTimeSeconds()}:R>"
                + $"\r\nAuto Close: `{ac.Timer}` hours");
        }
        if (embed.Fields.Count == 0) embed.AddField("None", "No open cases!");

        await new DiscordMessageBuilder().AddEmbed(embed).ModifyAsync(origMsg);
    }
}