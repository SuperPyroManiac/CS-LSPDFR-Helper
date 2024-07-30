using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Case;

public class JoinCase
{
    [Command("joincase")]
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
}