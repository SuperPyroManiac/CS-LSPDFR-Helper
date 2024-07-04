using DSharpPlus;
using DSharpPlus.EventArgs;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Processors.RPH;

namespace LSPDFR_Helper.Functions.AutoHelper;

public class MessageMonitor
{
    public static async Task MessageSentEvent(DiscordClient s, MessageCreatedEventArgs ctx)
    {
        if ( Program.Cache.GetCases().All(x => x.ChannelId != ctx.Channel.Id) ) return;
        var ac = Program.Cache.GetCases().First(x => x.ChannelId == ctx.Channel.Id);
        if (ac.OwnerId != ctx.Author.Id) return;
        
        if (Program.Cache.GetUser(ac.OwnerId).Blocked)
        {
            await ctx.Message.RespondAsync(BasicEmbeds.Error($"__You are blacklisted from the bot!__\r\n>>> Contact server staff in <#{Program.Settings.StaffContactChId}> if you think this is an error!"));
            await CloseCase.Close(ac);
            return;
        }
        
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
            if ( !acceptedFiles.Contains(attach.FileName) ) continue;
            if ( attach.FileSize / 1000000 > 3 )
            {
                await ctx.Message.RespondAsync(BasicEmbeds.Error("__Blacklisted!__\r\n>>> You have sent a log bigger than 3MB! You may not use the AutoHelper until staff review this!"));
                await Functions.Blacklist(ctx.Author.Id, 
                    $">>> **User:** {ctx.Author.Mention} ({ctx.Author.Id.ToString()})\r\n**Log:** {ctx.Message.JumpLink}\r\nUser sent a log greater than 3MB!\r\n**File Size:** {attach.FileSize/1000000}MB");
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
    }
}