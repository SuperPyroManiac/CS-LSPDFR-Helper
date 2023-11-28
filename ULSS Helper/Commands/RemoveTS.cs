using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;

namespace ULSS_Helper.Commands;

public class RemoveTs : ApplicationCommandModule
{
    [SlashCommand("RemoveTS", "Removes a TS from the database!")]
    [RequireBotAdmin()]
    public async Task RemoveTsCmd(InteractionContext ctx, [Option("ID", "User discord ID")] string id)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;

        var isValid = false;
        foreach (var ts in Database.LoadTs())
        {
            if (ts.ID == id)
            {
                Database.DeleteTs(ts);
                isValid = true;
                await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Success($"**Removed TS {ts.Username} with user ID: {ts.ID}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning($"**Removed TS {ts.Username} with user ID: {ts.ID}**"));
                return;
            }
        }
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!isValid)
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error($"**No TS found with id: {id}!**")));
        }
    }
}