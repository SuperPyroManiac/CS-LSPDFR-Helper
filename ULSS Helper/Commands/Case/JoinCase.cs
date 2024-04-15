using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Case;

public class JoinCase : ApplicationCommandModule
{
    [SlashCommand("JoinCase", "Joins an autohelper case!")]
    [RequireNotOnBotBlacklist]
    public async Task JoinCaseCmd(InteractionContext ctx,
        [Autocomplete(typeof(CaseAutoComplete)),Option("Case", "Must match an open case!")] string caseId)
    {
        await ctx.Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCase(caseId);
        var msg = new DiscordWebhookBuilder();
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

        var ch = await Program.Client.GetChannelAsync(ulong.Parse(acase.ChannelID));
        var th = (DiscordThreadChannel)ch;
        var usrs = await th.ListJoinedMembersAsync();
        var check = usrs.FirstOrDefault(usr => usr.Id == ctx.User.Id);
        if (check != null)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__Already joined!__\r\n" +
                $"You have already joined Case: `{caseId}`!\r\n" +
                $"See: <#{acase.ChannelID}>", true)));
            return;
        }
        
        await Public.AutoHelper.Modules.Case_Functions.JoinCase.PubJoin(acase, ctx.User.Id.ToString());
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Success(
            $"__Joined case!__\r\n" +
            $"You have been added to:\r\n" +
            $"<#{acase.ChannelID}>", true)));
    }
}