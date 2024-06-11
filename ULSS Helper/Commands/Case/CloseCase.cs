using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Commands.Case;

public class CloseCase
{
    [Command("CloseCase")]
    [Description("Close a case.")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The case - Type 'all' to close all cases!"), SlashAutoCompleteProvider<CaseAutoComplete>] string caseId)
    {
        if (!await PermissionManager.RequireTs(ctx)) return;
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseId);
        var msg = new DiscordInteractionResponseBuilder();
        msg.IsEphemeral = true;

        if (caseId.ToLower() == "all")
        {
            var count = 0;
            foreach (var aacase in Program.Cache.GetCasess())
            {
                if (aacase.Solved != 0) continue;
                Task.Delay(1000).GetAwaiter();
                await Public.AutoHelper.Modules.Case_Functions.CloseCase.Close(aacase, false);
                count++;
            }

            await CheckCases.Validate();
            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
                $"__All cases closed!__\r\n" +
                $">>> {count} cases were closed successfully!", true)));
            return;
        }
        
        if (acase == null)
        {
            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No case found!__\r\n" +
                $">>> Case: `{caseId}` does not exist!", true)));
            return;
        }
        
        if (acase.Solved == 1)
        {
            await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Case Is Closed!__\r\n" +
                $">>> Case: `{caseId}` has already been closed!", true)));
            return;
        }

        await Public.AutoHelper.Modules.Case_Functions.CloseCase.Close(acase);
        await ctx.EditResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
            $"__Case closed!__\r\n" +
            $">>> Case: <#{acase.ChannelID}> was closed successfully!", true)));
    }
}