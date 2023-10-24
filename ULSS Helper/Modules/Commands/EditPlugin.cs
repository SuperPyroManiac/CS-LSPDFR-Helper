using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class EditPlugin : ApplicationCommandModule
{
    [SlashCommand("EditPlugin", "Edits a plugin in the database!")]
    public async Task Cmd(
        InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string pN, 
        [Option("New_State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB")] State? pS = null)
    {
        if (ctx.Member.Roles.All(role => role.Id != Settings.GetTSRole()))
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

        Program.PlugName = plugin.Name;
        if (pS != null)
        {
            Program.PlugState = (State) pS;
        }
        else
        {
            if (Enum.TryParse(plugin.State, out State newState))
            {
                Program.PlugState = (State) newState;
            }
        }
        
        DiscordInteractionResponseBuilder modal = new();
        modal.WithTitle($"Editing {Program.PlugName} as {Program.PlugState.ToString()}").WithCustomId("edit-plugin").AddComponents(
            new TextInputComponent("Display Name:", "plugDName", required: true, style: TextInputStyle.Short, value: plugin.DName));
        modal.AddComponents(new TextInputComponent("Version:", "plugVersion", required: false,
            style: TextInputStyle.Short, value: plugin.Version));
        modal.AddComponents(new TextInputComponent("Early Access Version:", "plugEAVersion", required: false,
            style: TextInputStyle.Short, value: plugin.EAVersion));
        modal.AddComponents(new TextInputComponent("ID (on lcpdfr.com):", "plugID", required: false, style: TextInputStyle.Short, value: plugin.ID));
        modal.AddComponents(new TextInputComponent("Link:", "plugLink", required: false, style: TextInputStyle.Short, value: plugin.Link));
        
        await ctx.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    }
}