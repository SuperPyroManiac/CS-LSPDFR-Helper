using System.ComponentModel;
using System.Xml.Serialization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Messages.ModifiedProperties;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands;

[Command("errors")]
[Description("Error Commands!")]
public class Errors
{
    //===//===//===////===//===//===////===//Add Error//===////===//===//===////===//===//===//
    [Command("add")]
    [Description("Adds an error to the database!")]
        public async Task AddErrorCmd(SlashCommandContext ctx, [Description("Error Level")] Level level)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var error = new CustomTypes.MainTypes.Error
        {
            Pattern = "- REQUIRED -",
            Solution = "- REQUIRED -",
            Description = "- REQUIRED -",
            StringMatch = false,
            Level = level
        };
        var errorValues = new List<DiscordSelectComponentOption>
        {
            new("Pattern", "Error Pattern"),
            new("Solution", "Error Solution"),
            new("Description", "Error Description"),
            new("String Match", "Error String Match"),
        };

        var embed = BasicEmbeds.Info(
            $"__Adding New {error.Level} Error!__\r\n" +
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
        var bd = new DiscordInteractionResponseBuilder();
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectErrorValueToEdit,
                placeholder: "Edit Value",
                options: errorValues
            ));
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
            if (oldEditor != null) await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectErrorValueToEdit, new InteractionCache(ctx.Interaction, error, msg));
    }
        
    //===//===//===////===//===//===////===//Edit Error//===////===//===//===////===//===//===//
    [Command("edit")]
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
    
    //===//===//===////===//===//===////===//Remove Error//===////===//===//===////===//===//===//
    [Command("remove")]
    [Description("Removes an error from the database!")]
    public async Task RemoveErrorCmd
        (SlashCommandContext ctx, [Description("Must match an existing error id!")] string errorId)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var error = DbManager.GetError(errorId);
        if (error != null)
        {
            await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource, 
                bd.AddEmbed(BasicEmbeds.Success($"**Removed error with id: {errorId}**")));
            await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning(
                $"Removed error: {errorId}!\r\n>>> " +
                $"**Pattern:**\r\n" +
                $"```{error.Pattern}```\r\n" +
                $"**Solution:**\r\n" +
                $"```{error.Solution}```\r\n" +
                $"**Description:**\r\n" +
                $"```{error.Description}```\r\n" +
                $"**String Match:**\r\n" +
                $"```{error.StringMatch}```\r\n" +
                $"**Error Level: {error.Level}**"));
            DbManager.DeleteError(error);
            return;
        }
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, 
            bd.AddEmbed(BasicEmbeds.Error($"**No error found with id: {errorId}!**")));
    }
    
    //===//===//===////===//===//===////===//Find Error//===////===//===//===////===//===//===//
    [Command("find")]
    [Description("Returns a list of all errors in the database that match the search parameters!")]
    public static async Task FindErrorsCmd
    (SlashCommandContext ctx,
        [Description("The error id in the bot's database.")] string id = null,
        [Description("Pattern for detecting the error.")] string pattern = null,
        [Description("Solution for the error.")] string solution = null,
        [Description("Description for the error.")] string description = null,
        [Description("Error level")] Level? level = null,
        [Description("true = enabled, false = disabled (approximate search)")] bool exactmatch = false)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        await ctx.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder { IsEphemeral = true });
        
        try 
        {
            var errorsFound = DbManager.FindErrors(id, pattern, solution, description, level, exactmatch);

            if (errorsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindErrorMessages.GetSearchParamsList($"### __Found {errorsFound.Count} error{(errorsFound.Count != 1 ? "s" : "")} that match{(errorsFound.Count == 1 ? "es" : "")} the following search parameters:__",
                    id,
                    pattern,
                    solution,
                    description,
                    level,
                    exactmatch
                ) + "\r\nSearch results:";

                var currentPageContent = searchResultsHeader;
                for(var i=0; i < errorsFound.Count; i++)
                {
                    var error = errorsFound[i];
                    currentPageContent += "\r\n\r\n"
                        + $"> ### __Error Id {error.Id}__\r\n"
                        + $"> **Pattern:**\r\n> `{error.Pattern.Replace("\n", "`\n> `")}`\r\n> \r\n" 
                        + $"> **Solution:**\r\n> {error.Solution.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **Description:**\r\n> {error.Description.Replace("\n", "\n> ")}\r\n> \r\n"
                        + $"> **String Match:** `{error.StringMatch}`\r\n"
                        + $"> **Level:**\r\n> {error.Level}";
                    currentResultsPerPage++;
                    if (currentResultsPerPage == resultsPerPage || i == errorsFound.Count-1) {
                        var embed = BasicEmbeds.Generic(currentPageContent, DiscordColor.DarkBlue);
                        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Showing results {i+2 - currentResultsPerPage} - {i+1} (total: {errorsFound.Count})"
                        };
                        var page = new Page(embed: embed);
                        pages.Add(page);
                        currentPageContent = searchResultsHeader;
                        currentResultsPerPage = 0;
                    }
                }

                await ctx.Interaction.SendPaginatedResponseAsync(true, ctx.User, pages, asEditResponse: true);
            }
            else 
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning(
                    FindErrorMessages.GetSearchParamsList("No errors found with the following search parameters:", id, pattern, solution, description, level, exactmatch))));
            }
        } 
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
        }
    }
    
    //===//===//===////===//===//===////===//Find Error//===////===//===//===////===//===//===//
    [Command("export")]
    [Description("Exports all errors as an xml!")]
    public async Task ExportErrorsCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var errors = DbManager.GetErrors().ToArray();
        var serializer = new XmlSerializer(typeof(CustomTypes.MainTypes.Error[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder("Exports"), "ErrorExport.xml")))
        {
            serializer.Serialize(writer, errors);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success("Errors Exported.."));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Exported errors!__"));
    }
}