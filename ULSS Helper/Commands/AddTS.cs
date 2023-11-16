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
Console.WriteLine("-1");
        if (ctx.Member.Id != 339550607847194624)
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        } 
Console.WriteLine("0");

        var allowint = 0;
        if (allow) allowint = 1;
Console.WriteLine("1");
        var ts = new TS
        {
            ID = int.Parse(id),
            Username = ctx.Guild.GetMemberAsync(ulong.Parse(id)).Result.Username,
            View = 0,
            Allow = allowint
        };
Console.WriteLine("2");
        Database.AddTS(ts);
Console.WriteLine("3");
        await ctx.CreateResponseAsync(BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
    }
}