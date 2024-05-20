using System.Collections.Concurrent;
using ULSS_Helper.Objects;

namespace ULSS_Helper;

internal class Cache
{
    private Dictionary<ulong, ProcessCache> _processCacheDict = new();
    private Dictionary<string, UserActionCache> _userActionCacheDict = new();
    private ConcurrentDictionary<string, Plugin> _pluginCacheDict = new();
    private ConcurrentDictionary<string, Error> _errorCacheDict = new();
    private ConcurrentDictionary<string, AutoCase> _caseCacheDict = new();
    private ConcurrentDictionary<string, DiscordUser> _userCacheDict = new();

    /// <summary>Saves the plugin list to the cache replacing any old ones.</summary>
    internal void UpdatePlugins(List<Plugin> plugins)
    {
        _pluginCacheDict.Clear();
        foreach (var plugin in plugins) _pluginCacheDict.TryAdd(plugin.Name, plugin);
    }
    
    /// <summary>Returns a list of all cached plugins.</summary>
    internal List<Plugin> GetPlugins()
    {
        return _pluginCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single plugin based off the name.</summary>
    internal Plugin GetPlugin(string pluginName)
    {
        return _pluginCacheDict.GetValueOrDefault(pluginName);
    }
    
    /// <summary>Saves the error list to the cache replacing any old ones.</summary>
    internal void UpdateErrors(List<Error> errors)
    {
        _errorCacheDict.Clear();
        foreach (var error in errors) _errorCacheDict.TryAdd(error.ID, error);
    }
    
    /// <summary>Returns a list of all cached errors.</summary>
    internal List<Error> GetErrors()
    {
        return _errorCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single error based off the name.</summary>
    internal Error GetError(string errorId)
    {
        return _errorCacheDict.GetValueOrDefault(errorId);
    }
    
    /// <summary>Saves the case list to the cache replacing any old ones.</summary>
    internal void UpdateCases(List<AutoCase> cases)
    {
        _caseCacheDict.Clear();
        foreach (var acase in cases) _caseCacheDict.TryAdd(acase.CaseID, acase);
    }
    
    /// <summary>Returns a list of all cached cases.</summary>
    internal List<AutoCase> GetCasess()
    {
        return _caseCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single case based off the caseid.</summary>
    internal AutoCase GetCase(string caseId)
    {
        return _caseCacheDict.GetValueOrDefault(caseId);
    }
    
    /// <summary>Saves the user list to the cache replacing any old ones.</summary>
    internal void UpdateUsers(List<DiscordUser> users)
    {
        _userCacheDict.Clear();
        foreach (var user in users) _userCacheDict.TryAdd(user.UID, user);
    }
    
    /// <summary>Returns a list of all cached users.</summary>
    internal List<DiscordUser> GetUsers()
    {
        return _userCacheDict.Values.ToList();
    }
    
    /// <summary>Returns a single user based off the uid.</summary>
    internal DiscordUser GetUser(string userId)
    {
        return _userCacheDict.GetValueOrDefault(userId);
    }
    
    /// <summary>Saves the current state of a log analysis process.</summary>
    /// <param name="messageId">The ID of the last message in the current chain of messages in response to an uploaded log.</param>
    /// <param name="newCache">The ProcessCache object (will be merged with any existing cache objects for the same message id).</param>
    internal void SaveProcess(ulong messageId, ProcessCache newCache)
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
        var key = GetUserActionKey(userId, actionId);
        if (_userActionCacheDict.ContainsKey(key))
            _userActionCacheDict[key] = newCache;
        else
            _userActionCacheDict.TryAdd(key, newCache);
    }

    /// <summary>Gets the UserActionCache object for the given combination of user and action.</summary>
    /// <param name="userId">The user ID of the user who performs the action.</param>
    /// <param name="actionId">The ID of the action. This is usually the customId of a modal (accessible via <c>eventArgs.Interaction.Data.CustomId</c>).</param>
    internal UserActionCache GetUserAction(ulong userId, string actionId)
    {
        if (_userActionCacheDict.ContainsKey(GetUserActionKey(userId, actionId)))
            return _userActionCacheDict[GetUserActionKey(userId, actionId)];
        return null;
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
    internal async Task RemoveExpiredCacheEntries(TimeSpan maxCacheAge)
    {
        var expiredProcessKeys = _processCacheDict
            .Where(cache => (DateTime.Now - cache.Value.ModifiedAt) > maxCacheAge)
            .Select(cache => cache.Key)
            .ToList();

        foreach (var key in expiredProcessKeys)
        {
            _processCacheDict.Remove(key);
        }

        var expiredUserActionKeys = _userActionCacheDict
            .Where(cache => (DateTime.Now - cache.Value.ModifiedAt) > maxCacheAge)
            .Select(cache => cache.Key)
            .ToList();

        foreach (var key in expiredUserActionKeys)
        {
            try
            {
                if (_userActionCacheDict[key].Msg != null)
                    await _userActionCacheDict[key].Msg.DeleteAsync();
            }
            catch (Exception)
            {
                Console.WriteLine("Editor message was manually deleted!");
            }
            _userActionCacheDict.Remove(key);
        }
    }
}