using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddTS : ApplicationCommandModule
{
    [SlashCommand("AddTS", "Adds a TS to the database!")]
    public async Task AddTSCmd(InteractionContext ctx, [Option("ID", "User discord ID")] string id,
        [Option("Allow", "Allow access to the bot commands!")] bool allow)
    {
        if (!Program.Settings.Env.BotAdminUserIds.Any(adminId => adminId == ctx.Member.Id))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }

        if (Database.LoadTS().Any(ts => ts.ID == id))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("This TS already exists in the database!"));
            return;
        }

        var allowint = 0;
        if (allow) allowint = 1;
        var ts = new TS();
        ts.ID = id;
        ts.Username = ctx.Guild.GetMemberAsync(ulong.Parse(id)).Result.Username;
        ts.View = 0;
        ts.Allow = allowint;
        Database.AddTS(ts);
        await ctx.CreateResponseAsync(BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
    }
}