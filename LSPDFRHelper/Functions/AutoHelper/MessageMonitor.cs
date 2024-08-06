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
                    await ctx.Message.RespondAsync(
                        BasicEmbeds.Error("__Blacklisted!__\r\n>>> You have sent a log bigger than 3MB! You may not use the AutoHelper until staff review this!"));
                    await Functions.Blacklist(ctx.Author.Id,
                        $">>> **User:** {ctx.Author.Mention} ({ctx.Author.Id.ToString()})\r\n**Log:** {ctx.Message.JumpLink}\r\nUser sent a log greater than 3MB!\r\n**File Size:** {attach.FileSize / 1000000}MB");
                }

                switch ( attach.FileName )
                {
                    case "RagePluginHook.log":
                        var rphProcessor = new RphProcessor();
                        rphProcessor.Log = await RPHValidater.Run(attach.Url);
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