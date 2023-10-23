using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class EditError : ApplicationCommandModule
{
    [SlashCommand("EditError", "Edits an error in the database!")]
    public async Task EditErrorCmd(InteractionContext ctx, [Option("ID", "Errors ID!")] string eI, [Option("Level", "Warning type (WARN, SEVERE")] Level lvl)
    {
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        if (!DatabaseManager.LoadErrors().Any(x => x.ID.ToString() == eI))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error($"No error found with ID: {eI}"));
            return;
        }

        var error = DatabaseManager.LoadErrors().FirstOrDefault(x => x.ID.ToString() == eI);

        Program.ErrId = eI;
        Program.ErrLevel = lvl;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Editing error ID: {Program.ErrId}!").WithCustomId("edit-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph, value: error.Regex));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph, value: error.Solution));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}