using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace LSPDFRHelper.CustomTypes.AutoCompleteTypes;

public class PluginAutoComplete : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx)
    {
        var plugins = new List<DiscordAutoCompleteChoice>();
        foreach ( var plug in Program.Cache.GetPlugins() )
        {
            if ( plugins.Count < 25 && plug.Name.Contains(ctx.Options.First().Value.ToString(), StringComparison.CurrentCultureIgnoreCase) )
                plugins.Add(new DiscordAutoCompleteChoice(plug.Name, plug.Name));
        }
        return ValueTask.FromResult((IEnumerable<DiscordAutoCompleteChoice>)plugins);
    }
}