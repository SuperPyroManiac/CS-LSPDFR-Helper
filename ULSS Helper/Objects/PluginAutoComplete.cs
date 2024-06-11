using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ULSS_Helper.Objects;

public class PluginAutoComplete : IAutoCompleteProvider
{
	public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext ctx)
	{
		Dictionary<string, object> plugins = new Dictionary<string, object>();
		foreach (var plug in Program.Cache.GetPlugins())
		{
			if (plugins.Count < 25 && plug.Name.ToLower().Contains(ctx.Options.First().Value!.ToString()!.ToLower())) 
				plugins.Add(plug.Name, plug.Name);
		}
        
		return ValueTask.FromResult((IReadOnlyDictionary<string, object>)plugins);
	}
}