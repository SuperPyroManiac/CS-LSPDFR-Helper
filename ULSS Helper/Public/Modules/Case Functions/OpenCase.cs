using DSharpPlus;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Public.AutoHelper;

public class OpenCase
{
    internal static async Task CreateCase(ulong userId)
    {
        await UpdateMsg();
    }

    internal static async Task UpdateMsg()
    {
        var cl = Program.Client;
        var ch = cl.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId).Result;
        List<DiscordMessage> msgPurge = [];
        DiscordMessage origMsg = null;
        var embed = BasicEmbeds.Public("# __ULSS AutoHelper__");
        await foreach (var msg in ch.GetMessagesAsync(100))
        {
            if (msg.Embeds.Count == 0)
            {
                msgPurge.Add(msg);
                continue;
            }
            if (!msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper")) msgPurge.Add(msg);
            if (msg.Embeds.FirstOrDefault()!.Description.Contains("ULSS AutoHelper"))
            {
                origMsg = msg;
            }
        }
        foreach (var msg in msgPurge)
        {
            await ch.DeleteMessageAsync(msg);
        }
        if (origMsg == null) origMsg = await ch.SendMessageAsync("Starting...");
        embed.AddField("Early Access",
            "AutoHelper is still a work in progress! It is not perfect and can never fully replace people!");
        embed.AddField("Do not abuse the bot!",
            "This is broad, sending altered logs, other files, etc. Your access will be revoked!");
        embed.AddField("No proxy support!",
            "Do not use information from this bot to help others. Instead redirect them here themselves.");
        embed.AddField("Do not upload other peoples logs!",
            "This is considered proxy support, your access will be revoked!");

        var dmsg = new DiscordMessageBuilder().AddEmbed(embed);
        dmsg.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "OpenCase", "Open Case", true));
        
        
        await dmsg.ModifyAsync(origMsg);
    }
}