using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Case;

public class CloseCase
{
    [Command("closecase")]
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
}