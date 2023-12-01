using ULSS_Helper.Modules;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private Dictionary<string, UserActionCache> _userActionCacheDict = new();

    /// <summary>Saves the current state of a log analysis process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    /// <param name="newCache">The ProcessCache object (will be merged with any existing cache objects for the same message id).</param>
    internal void SaveProcess(ulong messageId, ProcessCache newCache)
    {
        if (_processCacheDict.ContainsKey(messageId))
        {
            ProcessCache currentCache = GetProcess(messageId);
            _processCacheDict[messageId] = currentCache.Update(newCache);
        }
        else
            _processCacheDict.Add(messageId, newCache);
    }

    /// <summary>Gets the ProcessCache object identified by a message ID that is related to the process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    internal ProcessCache GetProcess(ulong messageId)
    {
        if (_processCacheDict.ContainsKey(messageId))
            return _processCacheDict[messageId]; 
            
        return null;
    }
    
    internal void RemoveProcess(ulong messageId)
    {
        _processCacheDict.Remove(messageId);
    }

    /// <summary>
    /// Saves the object instances related to an action (command + modal form) and the performing user in the cache.
    /// This cache only allows saving one user-action-combination at once. A user cannot execute the same command (with modal form) twice in parallel anyway (unlike with log analysis).
    /// </summary>
    /// <param name="userId">The user ID of the user who performs the action.</param>
    /// <param name="actionId">The ID of the action. This is usually the <c>modal.CustomId</c>.</param>
    /// <param name="newCache">The UserActionCache object.</param>
    internal void SaveUserAction(ulong userId, string actionId, UserActionCache newCache)
    {
        string key = GetUserActionKey(userId, actionId);
        if (_userActionCacheDict.ContainsKey(key))
            _userActionCacheDict[key] = newCache;
        else
            _userActionCacheDict.Add(key, newCache);
    }

    /// <summary>Gets the UserActionCache object for the given combination of user and action.</summary>
    /// <param name="userId">The user ID of the user who performs the action.</param>
    /// <param name="actionId">The ID of the action. This is usually the customId of a modal (accessible via <c>eventArgs.Interaction.Data.CustomId</c>).</param>
    internal UserActionCache GetUserAction(ulong userId, string actionId)
    {
        return _userActionCacheDict[GetUserActionKey(userId, actionId)];
    }

    private string GetUserActionKey(ulong userId, string actionId)
    {
        return userId.ToString() + "&" + actionId;
    }

    internal void RemoveUserAction(ulong userId, string actionId)
    {
        _userActionCacheDict.Remove(GetUserActionKey(userId, actionId));
    }

    /// <summary>Removes all cache entries from the dictionaries that are older than the maxCacheAge parameter.</summary>
    internal void RemoveExpiredCacheEntries(TimeSpan maxCacheAge)
    {
        List<ulong> expiredProcessKeys = _processCacheDict
            .Where(cache => (DateTime.Now - cache.Value.ModifiedAt) > maxCacheAge)
            .Select(cache => cache.Key)
            .ToList();

        foreach (ulong key in expiredProcessKeys)
        {
            _processCacheDict.Remove(key);
        }

        List<string> expiredUserActionKeys = _userActionCacheDict
            .Where(cache => (DateTime.Now - cache.Value.ModifiedAt) > maxCacheAge)
            .Select(cache => cache.Key)
            .ToList();

        foreach (string key in expiredUserActionKeys)
        {
            _userActionCacheDict.Remove(key);
        }
    }
}