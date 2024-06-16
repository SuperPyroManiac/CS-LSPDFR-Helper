using System.Collections.Concurrent;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.CustomTypes.CacheTypes;

internal class Cache
{
    private Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private Dictionary<string, InteractionCache> _interactionCacheDict = new();
    private readonly ConcurrentDictionary<ulong, User> _userCacheDict = new();

    /// <summary>Replaces all cache entries with the specified user list.</summary>
    internal void UpdateUsers(List<User> users)
    {
        _userCacheDict.Clear();
        foreach (var user in users) _userCacheDict.TryAdd(user.Id, user);
    }
    
    /// <summary>Returns a list of all cached users.</summary>
    internal List<User> GetUsers()
    {
        return _userCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single user based off the uid.</summary>
    internal User GetUser(ulong userId)
    {
        return _userCacheDict[userId];
    }

    /// <summary>Removed all interaction/process caches that have not been modified in 15 minutes.</summary>
    private void RemoveExpiredCaches()
    {
        var expiredInteractionKeys = _interactionCacheDict
            .Where(x => x.Value.Expire <= DateTime.Now)
            .Select(x => x.Key);
        foreach ( var key in expiredInteractionKeys ) _interactionCacheDict.Remove(key);
    }
    
    /// <summary>Returns a unique key for an interaction.</summary>
    private static string GetUserActionKey(ulong userId, string actionId)
    {
        return userId.ToString() + "&" + actionId;
    }
    
    /// <summary>
    /// Saves the object instances related to an action (command + modal form) and the performing user in the cache.
    /// This cache only allows saving one user-action-combination at once. A user cannot execute the same command (with modal form) twice in parallel anyway (unlike with log analysis).
    /// </summary>
    /// <param name="userId">The user ID of the user who performs the action.</param>
    /// <param name="actionId">The ID of the action. This is usually the <c>modal.CustomId</c>.</param>
    /// <param name="newCache">The UserActionCache object.</param>
    internal void SaveUserAction(ulong userId, string actionId, InteractionCache newCache)
    {
        var key = GetUserActionKey(userId, actionId);
        if (_interactionCacheDict.ContainsKey(key))
            _interactionCacheDict[key] = newCache;
        else
            _interactionCacheDict.TryAdd(key, newCache);
    }
    
    /// <summary>Gets the UserActionCache object for the given combination of user and action.</summary>
    /// <param name="userId">The user ID of the user who performs the action.</param>
    /// <param name="actionId">The ID of the action. This is usually the customId of a modal (accessible via <c>eventArgs.Interaction.Data.CustomId</c>).</param>
    internal InteractionCache GetUserAction(ulong userId, string actionId)
    {
        if (_interactionCacheDict.ContainsKey(GetUserActionKey(userId, actionId)))
            return _interactionCacheDict[GetUserActionKey(userId, actionId)];
        return null;
    }
    
    internal void RemoveUserAction(ulong userId, string actionId)
    {
        _interactionCacheDict.Remove(GetUserActionKey(userId, actionId));
    }
}