using System.ComponentModel;
using System.Xml.Serialization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;

namespace ULSS_Helper.Commands.Plugin;

public class ExportPlugins
{
    [Command("ExportPlugins")]
    [Description("Exports all plugins as an xml!")]
    public async Task ExportPluginsCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var plugins = Database.LoadPlugins().ToArray();
        var serializer = new XmlSerializer(typeof(Objects.Plugin[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder( "Exports"), "PluginExport.xml")))
        {
            serializer.Serialize(writer, plugins);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success("Plugins Exported.."));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Exported plugins!__", true));
    }
}