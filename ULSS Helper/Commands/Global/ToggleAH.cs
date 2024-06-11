using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Commands.Global;

public class ToggleAh
{
    [Command("ToggleAh")]
    [Description("Toggles if the AutoHelper is enabled or not!")]
    
    public async Task ToggleAhCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireBotAdmin(ctx)) return;
        var ahStatus = Database.AutoHelperStatus();
        ahStatus = !ahStatus;
        var newStatus = "0";
        if (ahStatus) newStatus = "1";
        Database.AutoHelperStatus(newStatus);

        await Startup.StartAutoHelper();
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        bd.AddEmbed(BasicEmbeds.Success($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
    }
}