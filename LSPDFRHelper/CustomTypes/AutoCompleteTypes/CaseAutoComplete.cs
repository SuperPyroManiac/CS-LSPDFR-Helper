using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace LSPDFRHelper.CustomTypes.AutoCompleteTypes;

public class CaseAutoComplete : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx)
    {
        var cases = new List<DiscordAutoCompleteChoice>();
        foreach (var acase in Program.Cache.GetCases().Where(c => !c.Solved))
        {
            if (acase.ServerId != ctx.Guild!.Id) continue;
            if (cases.Count < 25 && acase.CaseId.Contains(ctx.Options.First().Value!.ToString()!, StringComparison.CurrentCultureIgnoreCase) )
            {
                cases.Add(new DiscordAutoCompleteChoice($"{Program.Cache.GetUser(acase.OwnerId).Username} - Case: {acase.CaseId}", acase.CaseId));
            }
        }
        
        return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(cases);
    }
}