using System.Collections.Concurrent;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.CustomTypes.CacheTypes;

public class Cache
{
    private Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private Dictionary<string, InteractionCache> _interactionCacheDict = new();
    private readonly ConcurrentDictionary<ulong, User> _userCacheDict = new();
    private readonly ConcurrentDictionary<int, Error> _errorCacheDict = new();
    private readonly ConcurrentDictionary<string, Plugin> _pluginCacheDict = new();
    private readonly ConcurrentDictionary<string, AutoCase> _caseCacheDict = new();

    /// <summary>Replaces all cache entries with the specified plugin list.</summary>
    public void UpdatePlugins(List<Plugin> plugins)
    {
        _pluginCacheDict.Clear();
        foreach (var plugin in plugins) _pluginCacheDict.TryAdd(plugin.Name, plugin);
    }
    
    /// <summary>Returns a list of all cached plugins.</summary>
    public List<Plugin> GetPlugins()
    {
        return _pluginCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single plugin based off the name.</summary>
    public Plugin GetPlugin(string pluginname)
    {
        try
        { return _pluginCacheDict[pluginname]; }
        catch ( Exception e )
        { return null; }
    }
    
    /// <summary>Replaces all cache entries with the specified error list.</summary>
    public void UpdateErrors(List<Error> errors)
    {
        _errorCacheDict.Clear();
        foreach (var error in errors) _errorCacheDict.TryAdd(error.Id, error);
    }
    
    /// <summary>Returns a list of all cached errors.</summary>
    public List<Error> GetErrors()
    {
        return _errorCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single error based off the id.</summary>
    public Error GetError(int errorId)
    {
        return _errorCacheDict[errorId];
    }
    
    /// <summary>Replaces all cache entries with the specified user list.</summary>
    public void UpdateUsers(List<User> users)
    {
        _userCacheDict.Clear();
        foreach (var user in users) _userCacheDict.TryAdd(user.Id, user);
    }
    
    /// <summary>Returns a list of all cached users.</summary>
    public List<User> GetUsers()
    {
        return _userCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single user based off the uid.</summary>
    public User GetUser(ulong userId)
    {
        try
        { return _userCacheDict[userId]; }
        catch ( Exception e )
        { return null; }
    }

    /// <summary>Removed all interaction/process caches that have not been modified in 15 minutes.</summary>
    private void RemoveExpiredCaches()
    {
        var expiredInteractionKeys = _interactionCacheDict
            .Where(x => x.Value.Expire <= DateTime.Now)
            .Select(x => x.Key);
        foreach ( var key in expiredInteractionKeys ) _interactionCacheDict.Remove(key);
        
        var expiredProcessKeys = _processCacheDict
            .Where(x => x.Value.Expire <= DateTime.Now)
            .Select(x => x.Key);
        foreach ( var key in expiredProcessKeys ) _processCacheDict.Remove(key);
    }
    
    /// <summary>Saves the current state of a log analysis process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    /// <param name="newCache">The ProcessCache object (will be merged with any existing cache objects for the same message id).</param>
    public void SaveProcess(ulong messageId, ProcessCache newCache)
    {
        if (_processCacheDict.ContainsKey(messageId))
        {
            var currentCache = GetProcess(messageId);
            _processCacheDict[messageId] = currentCache.Update(newCache);
        }
        else
            _processCacheDict.Add(messageId, newCache);
    }
    
    /// <summary>Gets the ProcessCache object identified by a message ID that is related to the process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    public ProcessCache GetProcess(ulong messageId)
    {
        if (_processCacheDict.ContainsKey(messageId))
            return _processCacheDict[messageId]; 
            
        return null;
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
    public void SaveUserAction(ulong userId, string actionId, InteractionCache newCache)
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
    public InteractionCache GetUserAction(ulong userId, string actionId)
    {
        if (_interactionCacheDict.ContainsKey(GetUserActionKey(userId, actionId)))
            return _interactionCacheDict[GetUserActionKey(userId, actionId)];
        return null;
    }
    
    public void RemoveUserAction(ulong userId, string actionId)
    {
        _interactionCacheDict.Remove(GetUserActionKey(userId, actionId));
    }
}