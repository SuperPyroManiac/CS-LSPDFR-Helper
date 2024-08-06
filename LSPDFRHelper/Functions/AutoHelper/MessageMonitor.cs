using DSharpPlus;
using DSharpPlus.EventArgs;
using FuzzySharp;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Processors.ASI;
using LSPDFRHelper.Functions.Processors.ELS;
using LSPDFRHelper.Functions.Processors.RPH;
using LSPDFRHelper.Functions.Processors.XML;

namespace LSPDFRHelper.Functions.AutoHelper;

public class MessageMonitor
{
    public static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        if ( Program.Cache.GetCases().All(x => x.ChannelId != ctx.Channel.Id) ) return;
        var ac = Program.Cache.GetCases().First(x => x.ChannelId == ctx.Channel.Id);
        if (ac.OwnerId != ctx.Author.Id) return;
        if ( Program.Cache.GetUser(ac.OwnerId).Blocked ) return;
        
        foreach (var error in Program.Cache.GetErrors().Where(error => error.Level == Level.PMSG))
        {
            var ratio = Fuzz.WeightedRatio(ctx.Message.Content, error.Pattern);
            if ( ratio <= 85 ) continue;
            var emb = BasicEmbeds.Public(
                $"## __LSPDFR AutoHelper__\r\n>>> {error.Solution}");
            emb.Footer!.Text = emb.Footer.Text + $" - ID: {error.Id} - Match {ratio}%";
            await ctx.Message.RespondAsync(emb);
        }
        
        if (ctx.Message.Attachments.Count == 0) return;

        var acceptedFiles = new List<string> { "RagePluginHook.log", "ELS.log", "asiloader.log" };
        foreach ( var attach in ctx.Message.Attachments )
        {
            if ( acceptedFiles.Contains(attach.FileName) )
            {
                if ( attach.FileSize / 1000000 > 3 )
                {
                    await ctx.Message.RespondAsync(BasicEmbeds.Error("__Blacklisted!__\r\n>>> You have sent a log bigger than 3MB! Your access to the bot has been revoked. You can appeal this at https://dsc.PyrosFun.com"));
                    await Functions.Blacklist(ctx.Author.Id, $">>> **User:** {ctx.Author.Mention} ({ctx.Author.Id.ToString()})\r\n**Log:** [HERE]({attach.Url})\r\nUser sent a log greater than 3MB!\r\n**File Size:** {attach.FileSize / 1000000}MB\r\n**Server:** {ctx.Guild.Name} ({ctx.Guild.Id}\r\n**Channel:** {ctx.Channel.Name})");
                    return;
                }

                switch ( attach.FileName )
                {
                    case "RagePluginHook.log":
                        var rphProcessor = new RphProcessor();
                        rphProcessor.Log = await RPHValidater.Run(attach.Url);
                        if ( rphProcessor.Log.LogModified )
                        {
                            await ctx.Message.RespondAsync(BasicEmbeds.Warning("__Skipped!__\r\n>>> You have sent a modified log! Your log has been flagged as modified. If you renamed a file or this was an accident then you can disregard this."));
                            await Logging.ReportPubLog(BasicEmbeds.Warning($"__Possible Abuse__\r\n>>> **User:** {ctx.Author!.Mention} ({ctx.Author.Id})\r\n**Log:** [HERE]({attach.Url})\r\nUser sent a modified log!\r\n**File Size:** {attach.FileSize / 1000000}MB\r\n**Server:** {ctx.Guild.Name} ({ctx.Guild.Id}\r\n**Channel:** {ctx.Channel.Name})"));
                            return;
                        }
                        await rphProcessor.SendAutoHelperMessage(ctx);
                        break;
                    case "ELS.log":
                        var elsProcessor = new ELSProcessor();
                        elsProcessor.Log = await ELSValidater.Run(attach.Url);
                        await elsProcessor.SendAutoHelperMessage(ctx);
                        break;
                    case "asiloader.log":
                        var asiProcessor = new ASIProcessor();
                        asiProcessor.Log = await ASIValidater.Run(attach.Url);
                        await asiProcessor.SendAutoHelperMessage(ctx);
                        break;
                }
            }

            if ( attach.FileName!.EndsWith(".xml") || attach.FileName.EndsWith(".meta") )
            {
                var response = BasicEmbeds.Public($"## __AutoHelper XML Info__{BasicEmbeds.AddBlanks(35)}");
                response.AddField(attach.FileName, $"```xml\r\n{await XmlValidator.Run(attach.Url)}\r\n```");
                await ctx.Message.RespondAsync(response);
            }
        }
    }
}