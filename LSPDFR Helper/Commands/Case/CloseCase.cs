using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.AutoCompleteTypes;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Case;

public class CloseCase
{
    [Command("closecase")]
    [Description("Close a case.")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The case - Type 'all' to close all cases!"), SlashAutoCompleteProvider<CaseAutoComplete>] string caseid)
    {
        if (!await PermissionManager.RequireTs(ctx)) return;
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseid);
        var msg = new DiscordInteractionResponseBuilder();
        msg.IsEphemeral = true;

        if (caseid.Equals("all", StringComparison.CurrentCultureIgnoreCase) )
        {
            var count = 0;
            foreach (var aacase in Program.Cache.GetCases())
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
}