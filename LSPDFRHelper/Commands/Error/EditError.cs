using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands.Error;

public class EditError
{
    [Command("editerror")]
    [Description("Edits an error in the database!")]
    public async Task EditErrorCmd
    (SlashCommandContext ctx, 
        [Description("The error ID.")] string errorId,
        [Description("The error level.")] Level newlevel = default)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (DbManager.GetError(errorId) == null)
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Error($"No error found with ID: {errorId}")));
            return;
        }
        
        var error = DbManager.GetError(errorId);
        
        if (newlevel != default) error.Level = newlevel;
        var errorValues = new List<DiscordSelectComponentOption>
        {
            new("Pattern", "Error Pattern"),
            new("Solution", "Error Solution"),
            new("Description", "Error Description"),
            new("String Match", "Error String Match"),
        };

        var embed = BasicEmbeds.Info(
            $"__Editing Error ID: {error.Id}__\r\n" +
            $">>> **Pattern:**\r\n" +
            $"```{error.Pattern}```\r\n" +
            $"**Solution:**\r\n" +
            $"```{error.Solution}```\r\n" +
            $"**Description:**\r\n" +
            $"```{error.Description}```\r\n" +
            $"**String Match:**\r\n" +
            $"```{error.StringMatch}```\r\n" +
            $"**Error Level: {error.Level}**");
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectErrorValueToEdit,
                placeholder: "Edit Value",
                options: errorValues));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                CustomIds.SelectErrorValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":checkyes:"))));

        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, CustomIds.SelectErrorValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectErrorValueToEdit, new InteractionCache(ctx.Interaction, error, msg));
    }
}