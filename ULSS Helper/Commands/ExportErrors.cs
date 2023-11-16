using System.Xml.Serialization;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ULSS_Helper.Events;
using ULSS_Helper.Messages;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Commands;

public class ExportErrors : ApplicationCommandModule
{
    [SlashCommand("ExportErrors", "Exports all errors as an xml!")]

    public async Task ExportErrorsCmd(InteractionContext ctx)
    {
        if (ctx.Member.Roles.All(role => role.Id != Program.Settings.Env.TsRoleId))
        {
            await ctx.CreateResponseAsync(embed: BasicEmbeds.Error("You do not have permission for this!"));
            return;
        }
        
        var errors = Database.LoadErrors().ToArray();
        var serializer = new XmlSerializer(typeof(Error[]));
        await using (var writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml")))
        {
            serializer.Serialize(writer, errors);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml"), FileMode.Open, FileAccess.Read);
        await ctx.CreateResponseAsync(BasicEmbeds.Info("Exporting errors..."));
        await ctx.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .AddFile(fs, AddFileOptions.CloseStream));
        Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("Exported errors!"));
    }
}