using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace ULSS_Helper.Modules.Commands;

public class AddError : ApplicationCommandModule
{
    [SlashCommand("AddError", "Adds an error to the database!")]
    [SlashRequirePermissions(Permissions.ManageMessages)]
    public async Task AddErrorCmd(InteractionContext ctx, [Option("Level", "Warning type (WARN, SEVERE")] Level lvl)
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
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