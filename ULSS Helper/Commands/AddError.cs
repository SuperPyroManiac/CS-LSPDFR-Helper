using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddError : ApplicationCommandModule
{
    [SlashCommand("AddError", "Adds an error to the database!")]
    
    public async Task AddErrorCmd(InteractionContext ctx, [Option("Level", "Warning type (WARN, SEVERE, CRITICAL)")] Level lvl)
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }

        Program.ErrLevel = lvl;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Adding new {lvl.ToString()} error!").WithCustomId("add-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}