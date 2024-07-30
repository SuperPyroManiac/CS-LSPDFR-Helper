using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;

namespace LSPDFRHelper.Commands.Global;

public class Setup
{
    [Command("setup")]
    [Description("Adjust your server settings here!")]

    public async Task SetupCmd(SlashCommandContext ctx)
    {
        
    }
}