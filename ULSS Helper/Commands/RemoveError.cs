using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class RemoveError : ApplicationCommandModule
{
    [SlashCommand("RemoveError", "Removes an error from the database!")]

    public async Task RemoveErrorCmd(InteractionContext ctx,
        [Option("ID", "Must match an existing error id!")] string errId)
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
                BasicEmbeds.Warning("**TS attempted to remove error without permission.**"));
            return;
        }
        
        var isValid = false;
        foreach (var error in Database.LoadErrors())
        {
            if (error.ID == errId)
            {
                Database.DeleteError(error);
                isValid = true;
                await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Warning($"**Removed error with id: {errId}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning($"Removed error: {errId}!"));
                return;
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!isValid)
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"**No error found with id: {errId}!**")));
        }
    }
}