using System.Xml.Serialization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Modules.Commands;

public class ExportErrors : ApplicationCommandModule
{
    [SlashCommand("ExportErrors", "Exports all errors as an xml!")]
    public async Task ExportErrorsCmd(InteractionContext ctx)
    {
        var errors = DatabaseManager.LoadErrors().ToArray();
        var serializer = new XmlSerializer(typeof(Error[]));
        await using (var writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml")))
        {
            serializer.Serialize(writer, errors);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml"), FileMode.Open, FileAccess.Read);
        await ctx.CreateResponseAsync(MessageManager.Info("Exporting errors..."));
        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .AddFile(fs, AddFileOptions.CloseStream));
    }
}