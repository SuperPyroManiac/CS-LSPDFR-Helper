using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditError : ApplicationCommandModule
{
    [SlashCommand("EditError", "Edits an error in the database!")]

    public async Task EditErrorCmd(
        InteractionContext ctx, 
        [Option("ID", "Errors ID!")] string errorId, 
        [Option("New_Level", "Warning type (XTRA, WARN, SEVERE, CRITICAL)")] Level? newLevel=null
    )
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            return;
        }
        var ts = Database.LoadTs().FirstOrDefault(ts => ts.ID.ToString() == ctx.Member.Id.ToString());
        if (ts == null || ts.Allow == 0)
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
                BasicEmbeds.Warning("**TS attempted to edit error without permission.**"));
            return;
        }

        if (Database.LoadErrors().All(ts => ts.ID.ToString() != errorId))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"No error found with ID: {errorId}")));
            return;
        }

        var error = Database.GetError(errorId);

        if (newLevel != null)
        {
            error.Level = newLevel.ToString().ToUpper();
        }
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithCustomId("edit-error");
        modal.WithTitle($"Editing error ID: {error.ID}!");
        modal.AddComponents(new TextInputComponent(
            label: "Error Regex:", 
            customId: "errReg", 
            required: true, 
            style: TextInputStyle.Paragraph, 
            value: error!.Regex
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Error Solution:", 
            customId: "errSol", 
            required: true, 
            style: TextInputStyle.Paragraph, 
            value: error.Solution
        ));
        modal.AddComponents(new TextInputComponent(
            label: "Error Description:", 
            customId: "errDesc", 
            required: true, 
            style: TextInputStyle.Paragraph, 
            value: error.Description
        ));
        
		Program.Cache.SaveUserAction(ctx.Interaction.User.Id, modal.CustomId, new UserActionCache(ctx.Interaction, error));
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}