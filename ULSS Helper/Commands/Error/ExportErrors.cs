using System.ComponentModel;
using System.Xml.Serialization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using ULSS_Helper.Messages;
using ULSS_Helper.Modules.Functions;

namespace ULSS_Helper.Commands.Error;

public class ExportErrors
{
    [Command("ExportErrors")]
    [Description("Exports all errors as an xml!")]
    public async Task ExportErrorsCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var errors = Database.LoadErrors().ToArray();
        var serializer = new XmlSerializer(typeof(Objects.Error[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder( "Exports"), "ErrorExport.xml")))
        {
            serializer.Serialize(writer, errors);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "ErrorExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success("Errors Exported.."));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Exported errors!__", true));
    }
}