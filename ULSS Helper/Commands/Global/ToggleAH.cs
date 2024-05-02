using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Commands.Global;

public class ToggleAH : ApplicationCommandModule
{
    [SlashCommand("ToggleAH", "Toggles if the AutoHelper is enabled or not!")]
    [RequireBotAdmin]
    
    public async Task ToggleAHCmd(InteractionContext ctx)
    {
        var ahStatus = Database.AutoHelperStatus();
        ahStatus = !ahStatus;
        var newStatus = "0";
        if (ahStatus) newStatus = "1";
        Database.AutoHelperStatus(newStatus);

        await Startup.StartAutoHelper();
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        bd.AddEmbed(BasicEmbeds.Success($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
        await ctx.CreateResponseAsync(bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info($"__AutoHelper Status Changed!__\r\n> Enabled: {ahStatus}", true));
    }
}