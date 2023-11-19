using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddTs : ApplicationCommandModule
{
    [SlashCommand("AddTS", "Adds a TS to the database!")]
    public async Task AddTsCmd(InteractionContext ctx, [Option("ID", "User discord ID")] string id,
        [Option("Allow", "Allow access to the bot commands!")] bool allow)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        if (Program.Settings.Env.BotAdminUserIds.All(adminId => adminId != ctx.Member.Id))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("You do not have permission for this!")));
            return;
        }

        if (Database.LoadTs().Any(ts => ts.ID == id))
        {
            await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Error("This TS already exists in the database!")));
            return;
        }

        var allowint = 0;
        if (allow) allowint = 1;
        var ts = new TS
        {
	        ID = id,
	        Username = ctx.Guild.GetMemberAsync(ulong.Parse(id)).Result.Username,
	        View = 0,
	        Allow = allowint
        };
        Database.AddTs(ts);
        await ctx.CreateResponseAsync(bd.AddEmbed(BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**")));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
    }
}