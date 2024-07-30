using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace LSPDFRHelper.CustomTypes.AutoCompleteTypes;

public class CaseAutoComplete : IAutoCompleteProvider
{
    public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext ctx)
    {
        Dictionary<string, object> cases = new Dictionary<string, object>();
        foreach (var acase in Program.Cache.GetCases().Where(c => !c.Solved))
        {
            if (acase.ServerId != ctx.Guild!.Id) continue;
            if (cases.Count < 25 && acase.CaseId.Contains(ctx.Options.First().Value!.ToString()!, StringComparison.CurrentCultureIgnoreCase) )
            {
                cases.Add($"{Program.Cache.GetUser(acase.OwnerId).Username} - Case: {acase.CaseId}", acase.CaseId);
            }
        }
        
        return ValueTask.FromResult((IReadOnlyDictionary<string, object>)cases);
    }
}