using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands;

[Command("case")]
[Description("Case Commands!")]
public class Cases
{
    //===//===//===////===//===//===////===//Close Case//===////===//===//===////===//===//===//
    [Command("close")]
    [Description("Close a case.")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The case - Type 'all' to close all cases!"), SlashAutoCompleteProvider<CaseAutoComplete>] string caseid)
    {
        if (!await PermissionManager.RequireServerManager(ctx)) return;
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseid);
        if (acase != null) if ( acase.ServerId != ctx.Guild!.Id ) acase = null;
        var msg = new DiscordInteractionResponseBuilder();
        msg.IsEphemeral = true;

        if (caseid.Equals("all", StringComparison.CurrentCultureIgnoreCase) )
        {
            var count = 0;
            foreach (var aacase in Program.Cache.GetCases().Where(ac => ac.ServerId == ctx.Guild!.Id))
            {
                if (aacase.Solved) continue;
                await Functions.AutoHelper.CloseCase.Close(aacase);
                count++;
            }

            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
                $"__All cases closed!__\r\n" +
                $">>> {count} cases were closed successfully!")));
            return;
        }
        
        if (acase == null)
        {
            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No case found!__\r\n" +
                $">>> Case: `{caseid}` does not exist!")));
            return;
        }
        
        if (acase.Solved)
        {
            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Case Is Closed!__\r\n" +
                $">>> Case: `{caseid}` has already been closed!")));
            return;
        }

        await Functions.AutoHelper.CloseCase.Close(acase);
        await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
            $"__Case closed!__\r\n" +
            $">>> Case: <#{acase.ChannelId}> was closed successfully!")));
    }
    
    //===//===//===////===//===//===////===//Join Case//===////===//===//===////===//===//===//
    [Command("join")]
    [Description("Joins an autohelper case!")]
    public async Task JoinCaseCmd(SlashCommandContext ctx, [Description("The case"), SlashAutoCompleteProvider<CaseAutoComplete>] string caseid)
    {
        if (!await PermissionManager.RequireNotBlacklisted(ctx)) return;
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseid);
        if ( acase.ServerId != ctx.Guild!.Id ) acase = null;
        var msg = new DiscordWebhookBuilder();
        if (acase == null)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No case found!__\r\n" +
                $">>> Case: `{caseid}` does not exist!")));
            return;
        }
        if (acase.Solved)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Case Is Closed!__\r\n" +
                $">>> Case: `{caseid}` has already been closed!")));
            return;
        }
        if ( !await Functions.AutoHelper.JoinCase.Join(acase, ctx.User.Id, ctx.Guild!.Id) )
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Already joined!__\r\n>>> You have already joined case: `{acase.CaseId}`!\r\nSee: <#{acase.ChannelId}>")));
            return;
        }
        
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
            $"__Joined case!__\r\n" +
            $">>> You have been added to:\r\n" +
            $"<#{acase.ChannelId}>")));
    }
    
    //===//===//===////===//===//===////===//Find Case//===////===//===//===////===//===//===//
    [Command("find")]
    [Description("Finds autohelper cases a user opened!")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The user to check.")] DiscordUser userid)
    {
        if (!await PermissionManager.RequireNotBlacklisted(ctx)) return;
        await ctx. Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCases().Where(ac => ac.OwnerId.Equals(userid.Id)).ToList();
        var msg = new DiscordWebhookBuilder();
        
        if (acase.Count == 0)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No cases found!__\r\n" +
                $">>> User: {userid.Mention} does not have any cases!")));
            return;
        }
        
        acase.Sort((x, y) => DateTime.Compare(y.CreateDate, x.CreateDate));
        
        var embed = BasicEmbeds.Info(
            $"__Cases found!__\r\n" 
            + $"User: {userid.Mention} has opened {acase.Count} cases! Showing the most recent!");
        
        foreach (var ucase in acase)
        {
            if (embed.Fields.Count >= 24) break;
            embed.AddField($"__Case: {ucase.CaseId}__", 
                $">>> [Here](https://discord.com/channels/{ucase.ServerId}/{ucase.ChannelId})\r\n" +
                $"{Formatter.Timestamp(ucase.CreateDate.ToLocalTime())}\r\n" +
                $"TS Request: {Convert.ToBoolean(ucase.TsRequested)}", true);
        }
        
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(embed));
    }
    
}