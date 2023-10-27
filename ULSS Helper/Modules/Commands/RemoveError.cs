using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace ULSS_Helper.Modules.Commands;

public class RemoveError : ApplicationCommandModule
{
    [SlashCommand("RemoveError", "Removes an error from the database!")]
    [SlashRequirePermissions(Permissions.ManageMessages)]
    public async Task RemoveErrorCmd(InteractionContext ctx,
        [Option("ID", "Must match an existing error id!")] string errId)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }
        
        var isValid = false;
        foreach (var error in DatabaseManager.LoadErrors())
        {
            if (error.ID == errId)
            {
                DatabaseManager.DeleteError(error);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Warning($"**Removed error with id: {errId}**")));
                return;
            }
        }
        if (!isValid)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Error($"**No error found with id: {errId}!**")));
            return;
        }
    }
}