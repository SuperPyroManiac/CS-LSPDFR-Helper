using System.ComponentModel;
using System.Xml.Serialization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using LSPDFRHelper.CustomTypes.AutoCompleteTypes;
using LSPDFRHelper.CustomTypes.CacheTypes;
using LSPDFRHelper.CustomTypes.Enums;
using LSPDFRHelper.EventManagers;
using LSPDFRHelper.Functions;
using LSPDFRHelper.Functions.Messages;
using LSPDFRHelper.Functions.Messages.ModifiedProperties;
using LSPDFRHelper.Functions.Verifications;

namespace LSPDFRHelper.Commands;

[Command("plugins")]
[Description("Plugin Commands!")]
public class Plugins
{
    //===//===//===////===//===//===////===//Add Plugin//===////===//===//===////===//===//===//
    [Command("add")]
    [Description("Adds a plugin to the database!")]
    public async Task AddPluginCmd(SlashCommandContext ctx, 
        [Description("Plugins name as shown in the log!")] string pluginname, 
        [Description("Plugin type")] PluginType plugintype,
        [Description("Plugin state")] State pluginstate)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        
        if (DbManager.GetPlugins().Any(plugin => plugin.Name == pluginname))
        {
            var err = new DiscordInteractionResponseBuilder();
            err.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                err.AddEmbed(BasicEmbeds.Error("__This plugin already exists!__\r\n> Consider using /Plugins edit")));
            return;
        }

        CustomTypes.MainTypes.Plugin plugin = new()
        {
            Name = pluginname,
            DName = pluginname,
            Description = "N/A",
            PluginType = plugintype,
            State = pluginstate
        };
        
        DbManager.AddPlugin(plugin);
        await FindPluginMessages.SendDbOperationConfirmation(plugin, operation: DbOperation.CREATE,ctx.Interaction.ChannelId, ctx.Interaction.User.Id);
        
        var pluginValues = new List<DiscordSelectComponentOption>
        {
            new("Display Name", "Plugin DName"),
            new("Version", "Plugin Version"),
            new("Ea Version", "Plugin EaVersion"),
            new("Id", "Plugin Id"),
            new("Link", "Plugin Link"),
            new("Notes", "Plugin Notes"),
            new("Author Id", "Plugin AuthorId"),
            new("Announce", "Plugin Announce")
        };
        
        var embed = BasicEmbeds.Info(
            $"__Editing Plugin: {plugin.Name}__\r\n>>> " +
            $"**Display Name:** {plugin.DName}\r\n" +
            $"**Version:** {plugin.Version}\r\n" +
            $"**Ea Version:** {plugin.EaVersion}\r\n" +
            $"**Id:** {plugin.Id}\r\n" +
            $"**Link:** {plugin.Link}\r\n" +
            $"**Author Id:** {plugin.AuthorId}\r\n" +
            $"**Announce:** {plugin.Announce}\r\n" +
            $"**Notes:**\r\n" +
            $"```{plugin.Description}```\r\n" +
            $"**Type:** {plugin.PluginType}\r\n" +
            $"**State:** {plugin.State}");
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        
        var bd = new DiscordInteractionResponseBuilder();
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectPluginValueToEdit,
                placeholder: "Edit Value",
                options: pluginValues));
        bd.AddComponents(new DiscordButtonComponent(
            DiscordButtonStyle.Success,
            CustomIds.SelectPluginValueToFinish,
            "Done Editing",
            false,
            new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":checkyes:"))));
        
        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit);
            if (oldEditor != null) await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit, new InteractionCache(ctx.Interaction, plugin, msg));
    }
    
    //===//===//===////===//===//===////===//Edit Plugin//===////===//===//===////===//===//===//
    [Command("edit")]
    [Description("Edits a plugin in the database!")]
    public async Task EditPluginCmd
    (
        SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName,
        [Description("Plugin type.")] PluginType newtype = default,
        [Description("Plugin state.")] State newstate = default
    )
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();

        if (DbManager.GetPlugins().All(x => x.Name != pluginName))
        {
            bd.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"No plugin found with name {pluginName}")));
            return;
        }

        var plugin = DbManager.GetPlugin(pluginName);

        if (newstate != default) plugin.State = newstate;
        if (newtype != default) plugin.PluginType = newtype;
        
        var pluginValues = new List<DiscordSelectComponentOption>
        {
            new("Display Name", "Plugin DName"),
            new("Version", "Plugin Version"),
            new("Ea Version", "Plugin EaVersion"),
            new("Id", "Plugin Id"),
            new("Link", "Plugin Link"),
            new("Notes", "Plugin Notes"),
            new("Author Id", "Plugin AuthorId"),
            new("Announce", "Plugin Announce")
        };
        
        var embed = BasicEmbeds.Info(
            $"__Editing Plugin: {plugin.Name}__\r\n>>> " +
            $"**Display Name:** {plugin.DName}\r\n" +
            $"**Version:** {plugin.Version}\r\n" +
            $"**Ea Version:** {plugin.EaVersion}\r\n" +
            $"**Id:** {plugin.Id}\r\n" +
            $"**Link:** {plugin.Link}\r\n" +
            $"**Author Id:** {plugin.AuthorId}\r\n" +
            $"**Announce:** {plugin.Announce}\r\n" +
            $"**Notes:**\r\n" +
            $"```{plugin.Description}```\r\n" +
            $"**Type:** {plugin.PluginType}\r\n" +
            $"**State:** {plugin.State}");
        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
        {
            Text = $"Current Editor: {ctx.Interaction.User.Username}",
            IconUrl = ctx.User.AvatarUrl
        };
        bd.AddEmbed(embed);
        bd.AddComponents(
            new DiscordSelectComponent(
                customId: CustomIds.SelectPluginValueToEdit,
                placeholder: "Edit Value",
                options: pluginValues
            ));
        bd.AddComponents(
            new DiscordButtonComponent(
                DiscordButtonStyle.Success,
                CustomIds.SelectPluginValueToFinish,
                "Done Editing",
                false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":checkyes:"))));
        
        try
        {
            var oldEditor = Program.Cache.GetUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit);
            if (oldEditor != null)
                await oldEditor.Msg.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Someone manually deleted an editor message!");
        }
        
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        var msg = await ctx.Interaction.GetOriginalResponseAsync();
        Program.Cache.SaveUserAction(ctx.Interaction.User.Id, CustomIds.SelectPluginValueToEdit, new InteractionCache(ctx.Interaction, plugin, msg));
    }
    
    //===//===//===////===//===//===////===//Remove Plugin//===////===//===//===////===//===//===//
    [Command("remove")]
    [Description("Removes a plugin from the database!")]
    public async Task RemovePluginCmd(SlashCommandContext ctx, 
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string pluginName)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var isValid = false;
        foreach (var plugin in DbManager.GetPlugins().Where(x => x.Name == pluginName))
        {
            isValid = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd.AddEmbed(
                BasicEmbeds.Success($"**Removed plugin: {pluginName}**")));
            await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Warning(
                $"__Removed plugin: {pluginName}!__\r\n>>> " +
                $"**Display Name:** {plugin.DName}\r\n" +
                $"**Version:** {plugin.Version}\r\n" +
                $"**Ea Version:** {plugin.EaVersion}\r\n" +
                $"**Id:** {plugin.Id}\r\n" +
                $"**Link:** {plugin.Link}\r\n" +
                $"**Notes:**\r\n" +
                $"```{plugin.Description}```\r\n" +
                $"**Type:** {plugin.PluginType}\r\n" +
                $"**State:** {plugin.State}"));
            DbManager.DeletePlugin(plugin);
            return;
        }
        if (!isValid)
        {
            await ctx.Interaction.CreateResponseAsync( DiscordInteractionResponseType.ChannelMessageWithSource,
                bd.AddEmbed(BasicEmbeds.Error($"**No plugin found with name: {pluginName}!**")));
        }
    }
    
    //===//===//===////===//===//===////===//Find Plugin//===////===//===//===////===//===//===//
    [Command("find")]
    [Description("Returns a list of all plugins in the database that match the search parameters!")]
    public static async Task FindPluginsCmd(SlashCommandContext ctx,
        [Description("Must match an existing plugin name!"), 
         SlashAutoCompleteProvider<PluginAutoComplete>] string plugname=null,
        [Description("The plugin's display name.")] string plugdname=null,
        [Description("The plugin's id on lcpdfr.com.")] string plugid=null,
        [Description("The plugin's state.")] State? plugstate = null,
        [Description("The plugin's type.")] PluginType? plugtype = null,
        [Description("The plugin's description.")] string plugdescription=null,
        [Description("true = enabled, false = disabled (approximate search)")] bool exactmatch=false
        )
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder { IsEphemeral = true });
        
        try 
        {
            var pluginsFound = DbManager.FindPlugins(plugname, plugdname, plugid, plugstate, plugtype, plugdescription, exactmatch);

            if (pluginsFound.Count > 0) 
            {
                var resultsPerPage = 3;
                var currentResultsPerPage = 0;
                List<Page> pages = [];
                var searchResultsHeader = FindPluginMessages.GetSearchParamsList(
                    $"## __Found {pluginsFound.Count} plugin{(pluginsFound.Count != 1 ? "s" : "")} that match{(pluginsFound.Count == 1 ? "es" : "")} the following search parameters:__", 
                    plugname, 
                    plugdname, 
                    plugid, 
                    plugstate, 
                    plugtype,
                    plugdescription,
                    exactmatch
                ) + "\r\nSearch results:";

                var currentPageContent = searchResultsHeader;
                for(var i=0; i < pluginsFound.Count; i++)
                {
                    var plugin = pluginsFound[i];
                    var plugDesc = "N/A";
                    if (!string.IsNullOrEmpty(plugin.Description)) plugDesc = plugin.Description;
                    currentPageContent += "\r\n\r\n"
                        + $"> ### __Plugin: {plugin.Name}__\r\n"
                        + $"> **Display Name:** {plugin.DName}\r\n" 
                        + $"> **Version:** {plugin.Version}\r\n"
                        + $"> **Early Access Version:** {plugin.EaVersion}\r\n"
                        + $"> **Id (on lcpdfr.com):** {plugin.Id}\r\n"
                        + $"> **Link:** {plugin.Link}\r\n"
                        + $"> **Type:** {plugin.PluginType}\r\n"
                        + $"> **State:** {plugin.State}\r\n"
                        + $"> **Notes:** \r\n> {plugDesc.Replace("\n", "\n> ")}";
                    currentResultsPerPage++;
                    if (currentResultsPerPage == resultsPerPage || i == pluginsFound.Count-1) {
                        var embed = BasicEmbeds.Generic(currentPageContent, DiscordColor.DarkBlue);
                        embed.Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Showing results {i+2 - currentResultsPerPage} - {i+1} (total: {pluginsFound.Count})"
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
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Warning(FindPluginMessages.GetSearchParamsList("No plugins found with the following search parameters:", plugname, plugdname, plugid, plugstate, plugtype, plugdescription, exactmatch))));
            }
        }
        catch (InvalidDataException e)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(BasicEmbeds.Error(e.Message)));
        }
    }
    
    //===//===//===////===//===//===////===//Export Plugins//===////===//===//===////===//===//===//
    [Command("export")]
    [Description("Exports all plugins as an xml!")]
    public async Task ExportPluginsCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireBotEditor(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var plugins = DbManager.GetPlugins().ToArray();
        var serializer = new XmlSerializer(typeof(CustomTypes.MainTypes.Plugin[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder("Exports"), "PluginExport.xml")))
        {
            serializer.Serialize(writer, plugins);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success($"__Plugins Exported!__\r\n>>> Total: {plugins.Length}"));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Exported plugins!__"));
    }
}