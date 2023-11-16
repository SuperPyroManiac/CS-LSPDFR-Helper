using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class RemoveTS : ApplicationCommandModule
{
    [SlashCommand("RemoveTS", "Removes a TS from the database!")]

    public async Task AddErrorCmd(InteractionContext ctx, [Option("ID", "User discord ID")] ulong id)
    {
        if (ctx.Member.Id != 339550607847194624)
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        
        var isValid = false;
        foreach (var ts in Database.LoadTS())
        {
            if (ts.ID == id)
            {
                Database.DeleteTS(ts);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning($"**Removed TS {ts.Username} with user ID: {ts.ID}**")));
                Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning($"**Removed TS {ts.Username} with user ID: {ts.ID}**"));
                return;
            }
        }
        if (!isValid)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error($"**No TS found with id: {id}!**")));
            return;
        }
    }
}