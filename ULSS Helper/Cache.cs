using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private readonly Dictionary<ulong, ProcessCache> _processCache = new();

    internal void SaveProcess(ulong messageId, ProcessCache processCache)
    {
        if (_processCache.Any(cache => cache.Key == messageId))
            _processCache.Remove(messageId);
        _processCache.Add(messageId, processCache);
    }

    internal ProcessCache GetProcessCache(ulong messageId)
    {
        return _processCache[messageId];
    }
}