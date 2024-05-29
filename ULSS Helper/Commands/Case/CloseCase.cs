using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;
using ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

namespace ULSS_Helper.Commands.Case;

public class CloseCase : ApplicationCommandModule
{
    [SlashCommand("CloseCase", "Closes an autohelper case!")]
    [RequireTsRoleSlash]
    public async Task CloseCaseCmd(InteractionContext ctx,
        [Autocomplete(typeof(CaseAutoComplete)),Option("Case", "Must match an open case!")] string caseId)
    {
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseId);
        var msg = new DiscordWebhookBuilder();

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
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
                $"__All cases closed!__\r\n" +
                $"{count} cases were closed successfully!", true)));
            return;
        }
        
        if (acase == null)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No case found!__\r\n" +
                $"Case: `{caseId}` does not exist!", true)));
            return;
        }
        
        if (acase.Solved == 1)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Case Is Closed!__\r\n" +
                $"Case: `{caseId}` has already been closed!", true)));
            return;
        }

        await Public.AutoHelper.Modules.Case_Functions.CloseCase.Close(acase);
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
            $"__Case closed!__\r\n" +
            $"Case: <#{acase.ChannelID}> was closed successfully!", true)));
    }
}