using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands.Error;

public class AddError
{
    [Command("AddError")]
    [Description("Adds an error to the database!")]
    public async Task AddErrorCmd(SlashCommandContext ctx, [Description("Error Level")] Level level)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var error = new Objects.Error
        {
            Regex = "- REQUIRED -",
            Solution = "- REQUIRED -",
            Description = "- REQUIRED -",
            Level = level.ToString().ToUpper()
        };
        var errorValues = new List<DiscordSelectComponentOption>()
        {
            new DiscordSelectComponentOption("Regex", "Error Regex"),
            new DiscordSelectComponentOption("Solution", "Error Solution"),
            new DiscordSelectComponentOption("Description", "Error Description"),
        };

        var embed = BasicEmbeds.Info(
            $"__Adding New {error.Level} Error!__\r\n" +
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
        var bd = new DiscordInteractionResponseBuilder();
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: ComponentInteraction.SelectErrorValueToEdit,
                placeholder: "Edit Value",
                options: errorValues
            ));
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