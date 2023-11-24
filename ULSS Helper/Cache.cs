using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private readonly Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private readonly Dictionary<string, UserActionCache> _userActionCacheDict = new();

    /// <summary>Saves the current state of a log analysis process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    /// <param name="newCache">The ProcessCache object (will be merged with any existing cache objects for the same message id).</param>
    internal void SaveProcess(ulong messageId, ProcessCache newCache)
    {
        if (_processCacheDict.Any(cache => cache.Key == messageId))
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
        return _processCacheDict[messageId];
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
        if (_userActionCacheDict.Any(cache => cache.Key == GetUserActionKey(userId, actionId)))
            _userActionCacheDict[GetUserActionKey(userId, actionId)] = newCache;
        else
            _userActionCacheDict.Add(GetUserActionKey(userId, actionId), newCache);
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

    // Removes all cache entries from the dictionaries that are older than 3 hours.
    internal void RemoveExpiredCacheEntries()
    {
        TimeSpan maxCacheAge = TimeSpan.FromTicks(TimeSpan.TicksPerHour * 3); // = 3 hours

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