﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class EditTS : ApplicationCommandModule
{
    [SlashCommand("ChangeErrorView", "Edits what you see in more details!")]
    public async Task EditViewCmd(
        InteractionContext ctx, 
        [Option("View", "0 all, 1 warn+, 2 severe+, 3 crit only, 4 none")] long view)
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        if (Database.LoadTS().All(x => x.ID.ToString() != ctx.Member.Id.ToString()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error($"You are not in the DB, please contact SuperPyroManiac!"));
            return;
        }
        if ((int)view > 4)
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error($"Your input is invalid!"));
            return;
        }

        var ts = Database.LoadTS().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
        ts.View = (int)view;
        ts.Username = ctx.Guild.GetMemberAsync(ulong.Parse(ts.ID)).Result.Username;
        
        Database.EditTS(ts);
        
        await ctx.CreateResponseAsync(embed: BasicEmbeds.Info($"<@{ctx.Member.Id.ToString()}>: You have changed your view type to: {view}"));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Info($"**<@{ctx.Member.Id.ToString()}> You has changed their view type to: {view}**"));
    }
    
    [SlashCommand("AllowPerms", "Allows a TS to use commands!")]
    public async Task EditAllowCmd(
        InteractionContext ctx, 
        [Option("ID", "User ID to change!")] string id,
        [Option("Allow", "Allow access to advanced bot commands!")] bool allow)
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        if (Database.LoadTS().All(x => x.ID.ToString() != id))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error($"User is not in the DB!"));
            return;
        }
        var allowint = 0;
        if (allow) allowint = 1;

        var ts = Database.LoadTS().FirstOrDefault(x => x.ID.ToString() == id);
        ts.Allow = allowint;
        ts.Username = ctx.Guild.GetMemberAsync(ulong.Parse(ts.ID)).Result.Username;
        
        Database.EditTS(ts);
        
        await ctx.CreateResponseAsync(embed: BasicEmbeds.Warning($"<@{id}>'s advanced command perms have been set to: {allow}"));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
            BasicEmbeds.Warning($"**<@{id}>'s advanced command perms have been set to: {allow}**"));
    }
}