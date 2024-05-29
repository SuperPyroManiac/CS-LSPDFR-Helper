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
        var parentChTh = await parentCh.ListPublicArchivedThreadsAsync(null, 100);
        var thList = parentChTh.Threads.ToList();
        thList.AddRange(parentCh.Threads);
        foreach (var th in thList)
        {
            caseChannelDict.TryAdd(th, Program.Cache.GetCase(th.Name.Split(": ")[1]));
            //Console.WriteLine($"{th.Name} --- {th.Name.Split(": ")[1]}");
        }
        Console.WriteLine($"================={thList.Count} Threads==================");

        foreach (var pair in caseChannelDict.Where(c => c.Value != null))
        {
            if (pair.Key.ThreadMetadata.IsArchived && pair.Value.Solved == 0) await CloseCase.Close(pair.Value);
            if (pair.Key.ThreadMetadata.IsArchived == false && pair.Value.Solved == 1) await CloseCase.Close(pair.Value);
            if (pair.Key.ThreadMetadata.IsArchived) continue;
            if (pair.Value.Solved == 1 || pair.Value.Timer == 0) await CloseCase.Close(pair.Value);
        }
        await CaseMonitor.UpdateMonitor();
    }
}