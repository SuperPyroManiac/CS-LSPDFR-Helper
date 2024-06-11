using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using DiscordUser = DSharpPlus.Entities.DiscordUser;

namespace ULSS_Helper.Commands.Case;

public class FindCases
{
    [Command("FindCases")]
    [Description("Finds autohelper cases a user opened!")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The user to check.")] DiscordUser userId)
    {
        await ctx. Interaction.DeferAsync(true);
        var acase = Program.Cache.GetCasess().Where(ac => ac.OwnerID.Equals(userId.Id.ToString())).ToList();
        var msg = new DiscordWebhookBuilder();
        
        if (acase.Count == 0)
        {
            await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(BasicEmbeds.Error(
                $"__No cases found!__\r\n" +
                $"User: <@{userId}> does not have any cases!", true)));
            return;
        }
        
        acase.Sort((x, y) => DateTime.Compare(y.CreateDate, x.CreateDate));
        
        var embed = BasicEmbeds.Info(
            $"__Cases found!__\r\n" 
            + $"User: {userId.Mention} has opened {acase.Count} cases! Showing the most recent!", true);
        
        foreach (var ucase in acase)
        {
            if (embed.Fields.Count >= 24) break;
            embed.AddField($"__Case: {ucase.CaseID}__", 
                $">>> <#{ucase.ChannelID}>\r\n" +
                $"{Formatter.Timestamp(ucase.CreateDate.ToLocalTime())}\r\n" +
                $"TS Request: {Convert.ToBoolean(ucase.TsRequested)}", true);
        }
        
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(embed));
    }
}