using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Global;

public class ToggleAh
{
    [Command("toggleah")]
    [Description("Toggles if the AutoHelper is enabled!")]
    
    public async Task ToggleAhCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireServerManager(ctx)) return;
        var ahStatus = !DbManager.AutoHelperStatus(ctx.Guild!.Id);
        DbManager.AutoHelperStatus(ctx.Guild!.Id, ahStatus);

        await AutoHelper.UpdateMainAhMessage(ctx.Guild.Id);
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        bd.AddEmbed(BasicEmbeds.Success($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
    }
}