using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddError : ApplicationCommandModule
{
    [SlashCommand("AddError", "Adds an error to the database!")]
    
    public async Task AddErrorCmd(InteractionContext ctx, [Option("Level", "Warning type (XTRA, WARN, SEVERE, CRITICAL)")] Level lvl)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            return;
        }
        var ts = Database.LoadTs().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
        if (ts == null || ts.Allow == 0)
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
                BasicEmbeds.Warning($"**TS attempted to add error without permission.**"));
            return;
        }

        Program.ErrLevel = lvl;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Adding new {lvl.ToString()} error!").WithCustomId("add-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Error Description:", "errDesc", required: true, style: TextInputStyle.Paragraph));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}