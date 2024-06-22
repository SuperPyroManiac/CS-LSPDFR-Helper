using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.CustomTypes.CacheTypes;
using LSPDFR_Helper.CustomTypes.Enums;
using LSPDFR_Helper.EventManagers;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Plugin;

public class AddPlugin
{
    [Command("addplugin")]
    [Description("Adds a plugin to the database!")]
    public async Task AddPluginCmd(SlashCommandContext ctx, 
        [Description("Plugins name as shown in the log!")] string pluginname, 
        [Description("Plugin type")] PluginType plugintype,
        [Description("Plugin state")] State pluginstate)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        
        if (DbManager.GetPlugins().Any(plugin => plugin.Name == pluginname))
        {
            var err = new DiscordInteractionResponseBuilder();
            err.IsEphemeral = true;
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                err.AddEmbed(BasicEmbeds.Error("__This plugin already exists!__\r\n> Consider using /EditPlugin", true)));
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
            $"**State:** {plugin.State}", true);
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
            new DiscordComponentEmoji(DiscordEmoji.FromName(Program.Client, ":yes:"))));
        
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
}