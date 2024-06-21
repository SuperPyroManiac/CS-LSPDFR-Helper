// using DSharpPlus.Commands.Processors.SlashCommands;
// using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
//
// namespace LSPDFR_Helper.CustomTypes.AutoCompleteTypes;
//
// public class CaseAutoComplete : IAutoCompleteProvider
// {
//     public ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext ctx)
//     {
//         Dictionary<string, object> cases = new Dictionary<string, object>();
//         foreach (var acase in Program.Cache.GetCasess().Where(c => c.Solved == 0))
//         {
//             if (cases.Count < 25 && acase.CaseID.ToLower().Contains(ctx.Options.First().Value!.ToString()!.ToLower()))
//             {
//                 cases.Add($"{Program.Cache.GetUser(acase.OwnerID).Username} - Case: {acase.CaseID}", acase.CaseID);
//             }
//         }
//         
//         return ValueTask.FromResult((IReadOnlyDictionary<string, object>)cases);
//     }
// }