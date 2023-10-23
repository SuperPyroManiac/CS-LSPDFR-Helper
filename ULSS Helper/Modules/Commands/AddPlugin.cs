using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class AddPlugin : ApplicationCommandModule
{
    [SlashCommand("AddPlugin", "Adds a plugin to the database!")]
    public async Task AddPluginCmd(InteractionContext ctx, 
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

        Program.PlugName = pN;
        Program.PlugState = pS;
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Adding {Program.PlugName} as {Program.PlugState.ToString()}").WithCustomId("add-plugin").AddComponents(
            new TextInputComponent("Display Name:", "plugDName", required: true, style: TextInputStyle.Short, value: Program.PlugName));
        modal.AddComponents(new TextInputComponent("Version:", "plugVersion", required: false,
            style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("Early Access Version:", "plugEAVersion", required: false,
            style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("ID (on lcpdfr.com):", "plugID", required: false, style: TextInputStyle.Short));
        modal.AddComponents(new TextInputComponent("Link:", "plugLink", required: false, style: TextInputStyle.Short));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}