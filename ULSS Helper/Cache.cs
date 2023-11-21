using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private readonly Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private readonly Dictionary<ulong, InteractionCache> _interactionCacheDict = new();

    internal void SaveProcess(ulong messageId, ProcessCache newCache)
    {
        if (_processCacheDict.Any(cache => cache.Key == messageId))
        {
            ProcessCache currentCache = GetProcess(messageId);
            _processCacheDict[messageId] = currentCache.Update(newCache);
            Console.WriteLine("Updated cache");
        }
        else
            _processCacheDict.Add(messageId, newCache);
    }

    internal ProcessCache GetProcess(ulong messageId)
    {
        return _processCacheDict[messageId];
    }

    internal void SaveInteraction(ulong interactionId, InteractionCache newCache)
    {
        if (_interactionCacheDict.Any(cache => cache.Key == interactionId))
            _interactionCacheDict[interactionId] = newCache;
        else
            _interactionCacheDict.Add(interactionId, newCache);
    }

    internal InteractionCache GetInteraction(ulong interactionId)
    {
        return _interactionCacheDict[interactionId];
    }
}