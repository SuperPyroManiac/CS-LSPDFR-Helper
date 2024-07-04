using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace LSPDFRHelper.CustomTypes.AutoCompleteTypes;

public class PluginAutoComplete : IAutoCompleteProvider
{
	public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext ctx)
	{
		var plugins = new Dictionary<string, object>();
		foreach (var plug in Program.Cache.GetPlugins())
		{
			if (plugins.Count < 25 && plug.Name.Contains(ctx.Options.First().Value!.ToString()!, StringComparison.CurrentCultureIgnoreCase) ) 
				plugins.Add(plug.Name, plug.Name);
		}
        
		return ValueTask.FromResult((IReadOnlyDictionary<string, object>)plugins);
	}
}