using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddError : ApplicationCommandModule
{
    [SlashCommand("AddError", "Adds an error to the database!")]
    [RequireAdvancedTsRole()]
    public async Task AddErrorCmd(InteractionContext ctx, [Option("Level", "Warning type (XTRA, WARN, SEVERE, CRITICAL)")] Level level)
    {
        Error error = new Error()
        {
            Level = level.ToString().ToUpper()
        };
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId("add-error");
        modal.WithTitle($"Adding new {error.Level} error!");
        modal.AddComponents(new TextInputComponent(
            label: "Error Regex:",
            customId: "errReg",
            required: true,
            style: TextInputStyle.Paragraph
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Error Solution:",
            customId: "errSol",
            required: true,
            style: TextInputStyle.Paragraph
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Error Description:",
            customId: "errDesc",
            required: true,
            style: TextInputStyle.Paragraph
        ));
        
		Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, error));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}