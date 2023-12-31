using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper;

public class AutoRPH
{
    internal static async Task ProccessLog(RPHLog log, MessageCreateEventArgs ctx, DiscordThreadChannel st)
    {
        var gtAver = "X";
        var lspdfRver = "X";
        var rpHver = "X";
        if (Program.Settings.Env.GtaVersion.Equals(log.GTAVersion)) gtAver = "\u2713";
        if (Program.Settings.Env.LspdfrVersion.Equals(log.LSPDFRVersion)) lspdfRver = "\u2713";
        if (Program.Settings.Env.RphVersion.Equals(log.RPHVersion)) rpHver = "\u2713";
        Thread.Sleep(100);
        var fs = new FileStream(Path.Combine(log.FilePath), FileMode.Open, FileAccess.Read);
        var msg = new DiscordMessageBuilder();
        msg.AddEmbed(BasicEmbeds.Public("## ULSS Auto Helper - BETA\r\n*Something should go here...*"));
        msg.AddFile(fs, AddFileOptions.CloseStream);
        await st.SendMessageAsync(msg);
                
        Thread.Sleep(30000);
        await st.DeleteAsync();
        await ctx.Message.DeleteAsync();
    }
}