using System.Xml.Serialization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class ExportPlugins : ApplicationCommandModule
{
    [SlashCommand("ExportPlugins", "Exports all plugins as an xml!")]
    public async Task ExportPluginsCmd(InteractionContext ctx)
    {
        var plugins = DatabaseManager.LoadPlugins().ToArray();
        var serializer = new XmlSerializer(typeof(Plugin[]));
        await using (var writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml")))
        {
            serializer.Serialize(writer, plugins);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml"), FileMode.Open, FileAccess.Read);
        await ctx.CreateResponseAsync(MessageManager.Info("Exporting plugins..."));
        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .AddFile(fs, AddFileOptions.CloseStream));
    }
}