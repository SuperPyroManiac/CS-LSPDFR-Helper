using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

public class PluginAutoComplete : IAutocompleteProvider
{
	public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
	{
		List<DiscordAutoCompleteChoice> plugins = new();
		foreach (var plug in Database.LoadPlugins())
		{
			if (plugins.Count < 25 && plug.Name.ToLower().Contains(ctx.FocusedOption.Value.ToString()!.ToLower())) plugins.Add(new DiscordAutoCompleteChoice(plug.Name, plug.Name));

		}
		return Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(plugins);
	}
}