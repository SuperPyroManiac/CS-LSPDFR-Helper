using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

public class PluginAutoComplete : IAutocompleteProvider
{
	public readonly List<Plugin> DbPlugs = Database.LoadPlugins();
	public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
	{
		List<DiscordAutoCompleteChoice> plugins = new();
		foreach (var plug in DbPlugs)
		{
			if (plugins.Count < 25 && plug.Name.Contains(ctx.FocusedOption.Value.ToString()!)) plugins.Add(new DiscordAutoCompleteChoice(plug.Name, plug.Name));
		}
		return plugins;
	}
}