using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ULSS_Helper.Objects;

public class CaseAutoComplete : IAutocompleteProvider
{
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        List<DiscordAutoCompleteChoice> cases = [];
        foreach (var acase in Program.Cache.GetCasess().Where(c => c.Solved == 0))
        {
            if (cases.Count < 25 && acase.CaseID.ToLower().Contains(ctx.FocusedOption.Value.ToString()!.ToLower()))
            {
                cases.Add(new DiscordAutoCompleteChoice($"{Program.Cache.GetUser(acase.OwnerID).Username} - Case: {acase.CaseID}", acase.CaseID));
            }
        }
        return Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(cases);
    }
}