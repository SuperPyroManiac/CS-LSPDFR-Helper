using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class AddPlugin : ApplicationCommandModule
{
    [SlashCommand("AddPlugin", "Adds a plugin to the database!")]
    
    public async Task AddPluginCmd(InteractionContext ctx, 
        [Option("Name", "Plugins name as shown in the log!")] string? pN, 
        [Option("State", "Plugin state, LSPDFR, EXTERNAL, BROKEN, LIB")] State pS)
    {
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        var ts = Database.LoadTS().FirstOrDefault(x => x.ID.ToString() == ctx.Member.Id.ToString());
        if (ts == null || ts.Allow == 0)
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id,
                BasicEmbeds.Warning($"**TS attempted to add plugin without permission.**"));
            return;
        }

        if (Database.LoadPlugins().Any(plugin => plugin.Name == pN))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("This plugin already exists in the database!\r\nConsider using /EditPlugin <Name> <State>"));
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