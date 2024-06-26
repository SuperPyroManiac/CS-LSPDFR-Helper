using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.Functions.Processors.RPH;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.ContextMenu;

public class AnalyzeLog
{
    [Command("Validate Log")]
    [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
    public async Task ValidateLogCmd(SlashCommandContext ctx, DiscordMessage targetMessage)
    {
        if (!await PermissionManager.RequireTs(ctx)) return;

        var log = await RPHValidater.Run(targetMessage.Attachments[0].Url);


        
        await ctx.RespondAsync("Cool");
    }
}