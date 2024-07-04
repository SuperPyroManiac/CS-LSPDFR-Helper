using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Processors.RPH;
using LSPDFR_Helper.Functions.Processors.XML;

namespace LSPDFR_Helper.Functions.AutoHelper;

public class MessageMonitor
{
    public static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        if ( Program.Cache.GetCases().All(x => x.ChannelId != ctx.Channel.Id) ) return;
        var ac = Program.Cache.GetCases().First(x => x.ChannelId == ctx.Channel.Id);
        if (ac.OwnerId != ctx.Author.Id) return;
        if ( Program.Cache.GetUser(ac.OwnerId).Blocked ) return;
        
        // foreach (var error in Program.Cache.GetErrors().Where(error => error.Level == "PMSG")) TODO: Fuzzymatch errors!
        // {
        //     var errregex = new Regex(error.Regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //     var errmatch = errregex.Match(ctx.Message.Content);
        //     if (errmatch.Success)
        //     {
        //         var emb = BasicEmbeds.Public(
        //             $"## __ULSS AutoHelper__\r\n>>> {error.Solution}");
        //         emb.Footer.Text = emb.Footer.Text + $" - ID: {error.ID}";
        //         await ctx.Message.RespondAsync(emb);
        //     }
        // }
        
        if (ctx.Message.Attachments.Count == 0) return;

        var acceptedFiles = new List<string> { "RagePluginHook.log", "ELS.log", "asiloader.log" };
        foreach ( var attach in ctx.Message.Attachments )
        {
            if ( !acceptedFiles.Contains(attach.FileName) )
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
                        break;
                    case "asiloader.log":
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