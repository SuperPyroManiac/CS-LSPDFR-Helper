using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.Functions.Messages;

namespace LSPDFRHelper.Commands.Case;

public class FindCases
{
    [Command("findcases")]
    [Description("Finds autohelper cases a user opened!")]
    public async Task CloseCaseCmd(SlashCommandContext ctx, [Description("The user to check.")] DiscordUser userid)
    {
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
                $">>> [Here](https://discord.com/channels/{Functions.Functions.GetGuild().Id}/{ucase.ChannelId})\r\n" +
                $"{Formatter.Timestamp(ucase.CreateDate.ToLocalTime())}\r\n" +
                $"TS Request: {Convert.ToBoolean(ucase.TsRequested)}", true);
        }
        
        await ctx.Interaction.EditOriginalResponseAsync(msg.AddEmbed(embed));
    }
}