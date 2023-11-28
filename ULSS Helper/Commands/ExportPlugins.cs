using System.Xml.Serialization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class ExportPlugins : ApplicationCommandModule
{
    [SlashCommand("ExportPlugins", "Exports all plugins as an xml!")]
    [RequireAdvancedTsRole]
    public async Task ExportPluginsCmd(InteractionContext ctx)
    {
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var plugins = Database.LoadPlugins().ToArray();
        var serializer = new XmlSerializer(typeof(Plugin[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder( "Exports"), "PluginExport.xml")))
        {
            serializer.Serialize(writer, plugins);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success("Plugins Exported.."));
        await ctx.CreateResponseAsync(bd);
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("Exported plugins!"));
    }
}