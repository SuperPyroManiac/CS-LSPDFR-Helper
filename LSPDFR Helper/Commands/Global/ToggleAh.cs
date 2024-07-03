using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Global;

public class ToggleAh
{
    [Command("toggleah")]
    [Description("Toggles if the AutoHelper is enabled!")]
    
    public async Task ToggleAhCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireBotAdmin(ctx)) return;
        var ahStatus = DbManager.AutoHelperStatus();
        ahStatus = !ahStatus;
        var newStatus = "0";
        if (ahStatus) newStatus = "1";
        DbManager.AutoHelperStatus(newStatus);

        await AutoHelper.UpdateMainAhMessage();
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        bd.AddEmbed(BasicEmbeds.Success($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}"));
    }
}