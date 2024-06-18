using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using PermissionManager = ULSS_Helper.Modules.Functions.PermissionManager;

namespace LSPDFR_Helper.Commands.Error;

public class EditError
{
    [Command("editerror")]
    [Description("Edits an error in the database!")]
    public async Task EditErrorCmd
    (SlashCommandContext ctx, 
        [Description("The error ID.")] string errorId,
        [Description("The error level.")] Level newLevel = default)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (DbManager.GetError(errorId) == null)
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Error($"No error found with ID: {errorId}")));
            return;
        }
        
        var error = DbManager.GetError(errorId);
        
        if (newLevel != default) error.Level = newLevel;
        var errorValues = new List<DiscordSelectComponentOption>()
        {
            new DiscordSelectComponentOption("Regex", "Error Regex"),
            new DiscordSelectComponentOption("Solution", "Error Solution"),
            new DiscordSelectComponentOption("Description", "Error Description"),
            new DiscordSelectComponentOption("String Match", "True: Pattern is String / False: Pattern is Regex or Fuzzymatch"),
        };

        var embed = BasicEmbeds.Info(
            $"__Editing Error ID: {error.Id}__\r\n" +
            $">>> **Regex:**\r\n" +
            $"```{error.Pattern}```\r\n" +
            $"**Solution:**\r\n" +
            $"```{error.Solution}```\r\n" +
            $"**Description:**\r\n" +
            $"```{error.Description}```\r\n" +
            $"**String Match:**\r\n" +
            $"```{error.StringMatch}```\r\n" +
            $"**Error Level: {error.Level}**", true);
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
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":yes:"))));

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