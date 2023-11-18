using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private Dictionary<ulong, ProcessCache> ProcessCache = new();

    internal void SaveProcess(ulong messageId, ProcessCache processCache)
    {
        if (ProcessCache.Any(cache => cache.Key == messageId))
            ProcessCache.Remove(messageId);
        ProcessCache.Add(messageId, processCache);
    }

    internal ProcessCache GetProcessCache(ulong messageId)
    {
        return ProcessCache[messageId];
    }
}