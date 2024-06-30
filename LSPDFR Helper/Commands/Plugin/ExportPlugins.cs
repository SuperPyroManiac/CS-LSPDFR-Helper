using System.ComponentModel;
using System.Xml.Serialization;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using LSPDFR_Helper.Functions;
using LSPDFR_Helper.Functions.Messages;
using LSPDFR_Helper.Functions.Verifications;

namespace LSPDFR_Helper.Commands.Plugin;

public class ExportPlugins
{
    [Command("exportplugins")]
    [Description("Exports all plugins as an xml!")]
    public async Task ExportPluginssCmd(SlashCommandContext ctx)
    {
        if (!await PermissionManager.RequireAdvancedTs(ctx)) return;
        var bd = new DiscordInteractionResponseBuilder();
        bd.IsEphemeral = true;
        
        var plugins = DbManager.GetPlugins().ToArray();
        var serializer = new XmlSerializer(typeof(CustomTypes.MainTypes.Plugin[]));
        await using (var writer = new StreamWriter(Path.Combine(Settings.GetOrCreateFolder("Exports"), "PluginExport.xml")))
        {
            serializer.Serialize(writer, plugins);
        }

        var fs = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "Exports", "PluginExport.xml"), FileMode.Open, FileAccess.Read);
        bd.AddFile(fs, AddFileOptions.CloseStream);
        bd.AddEmbed(BasicEmbeds.Success("Plugins Exported.."));
        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, bd);
        await Logging.SendLog(ctx.Interaction.Channel.Id, ctx.Interaction.User.Id, BasicEmbeds.Info("__Exported plugins!__"));
    }
}