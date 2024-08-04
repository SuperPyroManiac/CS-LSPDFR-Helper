using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands;

public class Setup
{
    [Command("setup")]
    [Description("Adjust your server settings here!")]

    public async Task SetupCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireServerAdmin(ctx)) return;

        var server = Program.Cache.GetServer(ctx.Guild!.Id);
        
        var bd = new DiscordInteractionResponseBuilder();
        
        if ( server.Blocked )
        {
            var res = new DiscordInteractionResponseBuilder();
            res.AddEmbed(BasicEmbeds.Error("__Server Blacklisted!__\r\n>>> If you think this is an error, you can contact the devs at https://dsc.PyrosFun.com"));
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, res);
            await Servers.Validate();
            return;
        }
        
        var serverValues = new List<DiscordSelectComponentOption>
        {
            new("AutoHelper Channel Id", "AhCh"),
            new("Monitor Channel Id", "MonitorCh"),
            new("Bot Manager Role Id", "ManagerRole")
        };

        var embed = BasicEmbeds.Info(
            $"__Editing Server Settings__\r\n*Leaving a field as '0' will disable it.*\r\n\r\n" +
            $">>> **AutoHelper Channel Id:**\r\n" +
            $"<#{server.AutoHelperChId}> \r\n(`{server.AutoHelperChId}`)\r\n" +
            $"**AutoHelper Monitor Id:**\r\n" +
            $"<#{server.MonitorChId}> \r\n(`{server.MonitorChId}`)\r\n" +
            $"**Bot Manager Role Id:**\r\n" +
            $"`{server.ManagerRoleId}`\r\n");
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectServerValueToEdit,
                placeholder: "Edit Value",
                options: serverValues));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                CustomIds.SelectServerValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":checkyes:"))));
        
        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, CustomIds.SelectServerValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectServerValueToEdit, new InteractionCache(ctx.Interaction, server, msg));
    }
}