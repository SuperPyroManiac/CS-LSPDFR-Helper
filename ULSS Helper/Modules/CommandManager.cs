using System.Xml;
using System.Xml.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace ULSS_Helper.Modules;

public class CommandManager : ApplicationCommandModule
{
    private static string? _plugName;
    private static string __errID;
    private static State _plugState;
    private static Level _errLevel;
    public enum State
    {
        [ChoiceName("LSPDFR")]
        LSPDFR,
        [ChoiceName("EXTERNAL")]
        EXTERNAL,
        [ChoiceName("BROKEN")]
        BROKEN,
        [ChoiceName("LIB")]
        LIB
    }
    public enum Level
    {
        [ChoiceName("WARN")]
        WARN,
        [ChoiceName("SEVERE")]
        SEVERE
    }

    [SlashCommand("AddPlugin", "Adds a plugin to the database!")]
    public async Task AddPlugin(InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string? pN, 
        [Option("State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB")] State pS)
    {
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        if (DatabaseManager.LoadPlugins().Any(plugin => plugin.Name == pN))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("This plugin already exists in the database!\r\nConsider using /EditPlugin <Name> <State>"));
            return;
        }

        _plugName = pN;
        _plugState = pS;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Adding {_plugName} as {_plugState.ToString()}").WithCustomId("add-plugin").AddComponents(
            new TextInputComponent("Display Name:", "plugDName", required: true, style: TextInputStyle.Short, value: _plugName));
        modal.AddComponents(new TextInputComponent("Version:", "plugVersion", required: false,
            style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("Early Access Version:", "plugEAVersion", required: false,
            style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("ID (on lcpdfr.com):", "plugID", required: false, style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("Link:", "plugLink", required: false, style: TextInputStyle.Short));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
    
    [SlashCommand("AddError", "Adds an error to the database!")]
    public async Task AddError(InteractionContext ctx, [Option("Level", "Warning type (WARN, SEVERE")] Level lvl)
    {
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        _errLevel = lvl;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Adding new {lvl.ToString()} error!").WithCustomId("add-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
    
    [SlashCommand("EditPlugin", "Edits a plugin in the database!")]
    public async Task EditPlugin(InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string pN, 
        [Option("State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB")] State pS)
    {
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        if (!DatabaseManager.LoadPlugins().Any(x => x.Name == pN))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error($"No plugin found with name {pN}"));
            return;
        }

        var plugin = DatabaseManager.LoadPlugins().FirstOrDefault(x => x.Name == pN);

        _plugName = plugin.Name;
        _plugState = pS;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Editing {_plugName} as {_plugState.ToString()}").WithCustomId("edit-plugin").AddComponents(
            new TextInputComponent("Display Name:", "plugDName", required: true, style: TextInputStyle.Short, value: plugin.DName));
        modal.AddComponents(new TextInputComponent("Version:", "plugVersion", required: false,
            style: TextInputStyle.Short, value: plugin.Version));
        modal.AddComponents(new TextInputComponent("Early Access Version:", "plugEAVersion", required: false,
            style: TextInputStyle.Short, value: plugin.EAVersion));
        modal.AddComponents(new TextInputComponent("ID (on lcpdfr.com):", "plugID", required: false, style: TextInputStyle.Short, value: plugin.ID));
        modal.AddComponents(new TextInputComponent("Link:", "plugLink", required: false, style: TextInputStyle.Short, value: plugin.Link));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
    
    [SlashCommand("EditError", "Edits an error in the database!")]
    public async Task EditError(InteractionContext ctx, [Option("ID", "Errors ID!")] string pN, [Option("Level", "Warning type (WARN, SEVERE")] Level lvl)
    {
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        if (!DatabaseManager.LoadErrors().Any(x => x.ID.ToString() == pN))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error($"No error found with ID: {pN}"));
            return;
        }

        var error = DatabaseManager.LoadErrors().FirstOrDefault(x => x.ID.ToString() == pN);

        __errID = pN;
        _errLevel = lvl;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Editing error ID: {__errID}!").WithCustomId("edit-error").AddComponents(
            new TextInputComponent("Error Regex:", "errReg", required: true, style: TextInputStyle.Paragraph, value: error.Regex));
        modal.AddComponents(new TextInputComponent("Error Solution:", "errSol", required: true, style: TextInputStyle.Paragraph, value: error.Solution));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }

    [SlashCommand("RemovePlugin", "Removes a plugin from the database!")]
    public async Task RemovePlugin(InteractionContext ctx,
        [Option("Name", "Must match an existing plugin name!")] string plugName)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }
        
        var isValid = false;
        foreach (var plugin in DatabaseManager.LoadPlugins())
        {
            if (plugin.Name == plugName)
            {
                DatabaseManager.DeletePlugin(plugin);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Warning($"**Removed: {plugName}**")));
                return;
            }
        }
        if (!isValid)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Error($"**No plugin found with name: {plugName}!**")));
            return;
        }
    }

    [SlashCommand("RemoveError", "Removes an error from the database!")]
    public async Task RemoveError(InteractionContext ctx,
        [Option("ID", "Must match an existing error id!")] string errId)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }
        
        var isValid = false;
        foreach (var error in DatabaseManager.LoadErrors())
        {
            if (error.ID == errId)
            {
                DatabaseManager.DeleteError(error);
                isValid = true;
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Warning($"**Removed error with id: {errId}**")));
                return;
            }
        }
        if (!isValid)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Error($"**No error found with id: {errId}!**")));
            return;
        }
    }

    [SlashCommand("ForceUpdateCheck", "Forced the database to update!")]
    public async Task ForceUpdate(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        if (ctx.Member.Roles.All(role => role.Id != 517568233360982017))
        {
            await ctx.CreateResponseAsync(embed: MessageManager.Error("You do not have permission for this!"));
            return;
        }

        var th = new Thread(DatabaseManager.UpdatePluginVersions);
        th.Start();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(MessageManager.Info("All plugin versions will be updated!")));
    }
    
    public static async Task PluginModal(DiscordClient s, ModalSubmitEventArgs e)
    {
        if (e.Interaction.Data.CustomId == "add-plugin")
        {
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin()
            {
                Name = _plugName,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = _plugState.ToString().ToUpper(),
                Link = plugLink
            };

            long dbRowId = DatabaseManager.AddPlugin(plug);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Added {_plugName}!**\r\n"
                    + $"DB Row ID: {dbRowId}\r\n"
                    + $"Display Name: {plugDName}\r\n" 
                    + $"Version: {plugVersion}\r\n"
                    + $"Early Access Version: {plugEaVersion}\r\n"
                    + $"ID (on lcpdfr.com): {plugId}\r\n"
                    + $"Link: {plugLink}\r\n"
                    + $"State: {_plugState}"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "add-error")
        {
            var err = new Error()
            {
                Regex = e.Values["errReg"],
                Solution = e.Values["errSol"],
                Level = _errLevel.ToString().ToUpper()
            };
            
            if (DatabaseManager.LoadErrors().Any(error => error.Regex == err.Regex))
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Error("This error already exists in the database!\r\nConsider using /EditError <ID>")));
                return;
            }

            long dbRowId = DatabaseManager.AddError(err);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Added a {err.Level} error with ID {dbRowId}**\r\n"
                    + $"Regex:\r\n```{err.Regex}```\r\n" 
                    + $"Solution:\r\n```{err.Solution}```"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "edit-plugin")
        {
            var plugDName = e.Values["plugDName"];
            var plugVersion = e.Values["plugVersion"];
            var plugEaVersion = e.Values["plugEAVersion"];
            var plugId = e.Values["plugID"];
            var plugLink = e.Values["plugLink"];

            var plug = new Plugin()
            {
                Name = _plugName,
                DName = plugDName,
                Version = plugVersion,
                EAVersion = plugEaVersion,
                ID = plugId,
                State = _plugState.ToString().ToUpper(),
                Link = plugLink
            };

            DatabaseManager.EditPlugin(plug);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Modified {_plugName}!**\r\n"
                    + $"Display Name: {plugDName}\r\n"
                    + $"Version: {plugVersion}\r\n"
                    + $"Early Access Version: {plugEaVersion}\r\n"
                    + $"ID (on lcpdfr.com): {plugId}\r\n"
                    + $"Link: {plugLink}\r\n"
                    + $"State: {_plugState}"
                ))
            );
        }
        
        if (e.Interaction.Data.CustomId == "edit-error")
        {
            var errReg = e.Values["errReg"];
            var errSol = e.Values["errSol"];

            var err = new Error()
            {
                ID = __errID,
                Regex = errReg,
                Solution = errSol,
                Level = _errLevel.ToString()
            };

            DatabaseManager.EditError(err);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(MessageManager.Info(
                    $"**Modified error ID: {__errID}!**\r\n"
                    + $"Regex: {errReg}\r\n"
                    + $"Solution: {errSol}\r\n"
                    + $"Level: {_errLevel.ToString()}"
                ))
            );
        }
    }
}