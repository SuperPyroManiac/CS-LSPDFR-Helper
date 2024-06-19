using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Error;

public class EditError
{
    [Command("EditError")]
    [Description("Edits an error in the database!")]
    public async Task EditErrorCmd
    (SlashCommandContext ctx, 
        [Description("The error ID.")] string errorId,
        [Description("The error level.")] Level? newLevel = null)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (Database.LoadErrors().All(ts => ts.ID.ToString() != errorId))
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Error($"No error found with ID: {errorId}")));
            return;
        }
        
        var error = Database.GetError(errorId);
        
        if (newLevel != null) error.Level = newLevel.ToString()!.ToUpper();
        var errorValues = new List<DiscordSelectComponentOption>()
        {
            new("Regex", "Error Regex"),
            new("Solution", "Error Solution"),
            new("Description", "Error Description"),
        };

        var embed = BasicEmbeds.Info(
            $"__Editing Error ID: {error.ID}__\r\n" +
            $">>> **Regex:**\r\n" +
            $"```{error.Regex}```\r\n" +
            $"**Solution:**\r\n" +
            $"```{error.Solution}```\r\n" +
            $"**Description:**\r\n" +
            $"```{error.Description}```\r\n" +
            $"**Error Level: {error.Level}**", true);
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: ComponentInteraction.SelectErrorValueToEdit,
                placeholder: "Edit Value",
                options: errorValues));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                ComponentInteraction.SelectErrorValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":yes:"))));

        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, ComponentInteraction.SelectErrorValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, ComponentInteraction.SelectErrorValueToEdit, new UserActionCache(ctx.Interaction, error, msg));
    }
}