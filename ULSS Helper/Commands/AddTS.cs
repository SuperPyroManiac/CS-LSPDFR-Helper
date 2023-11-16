﻿using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddTS : ApplicationCommandModule
{
    [SlashCommand("AddTS", "Adds a TS to the database!")]

    public async Task AddErrorCmd(InteractionContext ctx, [Option("ID", "User discord ID")] ulong id,
        [Option("Allow", "Allow access to the bot commands!")] bool allow)
    {
        if (ctx.Member.Id != 339550607847194624)
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }

        var allowint = 0;
        if (allow) allowint = 1;

        var ts = new TS();
        ts.ID = id;
        ts.Username = ctx.Guild.GetMemberAsync(id).Result.Username;
        ts.Allow = allowint;

        Database.AddTS(ts);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().AddEmbed(
                BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**")));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Warning($"**Added new TS {ts.Username} with user ID: {ts.ID}**"));
    }
}