using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditError : ApplicationCommandModule
{
    [SlashCommand("EditError", "Edits an error in the database!")]

    public async Task EditErrorCmd(
        InteractionContext ctx, 
        [Option("ID", "Errors ID!")] string eI, 
        [Option("New_Level", "Warning type (XTRA, WARN, SEVERE, CRITICAL)")] Level? lvl=null
    )
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }

        if (!Database.LoadErrors().Any(x => x.ID.ToString() == eI))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error($"No error found with ID: {eI}"));
            return;
        }

        var error = Database.LoadErrors().FirstOrDefault(x => x.ID.ToString() == eI);

        Program.ErrId = eI;
        if (lvl != null)
        {
            Program.ErrLevel = (Level) lvl;
        }
        else
        {
            if (Enum.TryParse(error.Level, out Level newLvl))
            {
                Program.ErrLevel = newLvl;
            }
        }
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Editing error ID: {Program.ErrId}!").WithCustomId("edit-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph, value: error.Regex));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph, value: error.Solution));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}