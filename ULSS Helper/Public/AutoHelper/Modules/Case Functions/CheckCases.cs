using System.Collections.Concurrent;
using DSharpPlus.Entities;
using ULSS_Helper.Objects;

namespace ULSS_Helper.Public.AutoHelper.Modules.Case_Functions;

internal class CheckCases
{
    internal static async Task Validate()
    {
    ConcurrentDictionary<DiscordThreadChannel, AutoCase> caseChannelDict = new();
        
        var parentCh = await Program.Client.GetChannelAsync(Program.Settings.Env.AutoHelperChannelId);
        foreach (var th in parentCh.Threads)
            caseChannelDict.TryAdd(th, Program.Cache.GetCase(th.Name.Split(": ")[1]));

        foreach (var pair in caseChannelDict)
        {
            if (pair.Key.ThreadMetadata.IsArchived && pair.Value.Solved == 0)
            {
                await CloseCase.Close(pair.Value);
                break;
            }
            if (!pair.Key.ThreadMetadata.IsArchived && pair.Value.Solved == 1)
            {
                await CloseCase.Close(pair.Value);
                break;
            }
            if (pair.Key.ThreadMetadata.IsArchived) continue;
            if (pair.Value.Solved == 1 || pair.Value.Timer == 0) await CloseCase.Close(pair.Value);
        }
        await CaseMonitor.UpdateMonitor();
    }
}