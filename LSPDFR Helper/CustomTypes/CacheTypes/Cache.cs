using System.Collections.Concurrent;
using LSPDFR_Helper.CustomTypes.MainTypes;

namespace LSPDFR_Helper.CustomTypes.CacheTypes;

internal class Cache
{
    private readonly ConcurrentDictionary<ulong, User> _userCacheDict = new();

    
    internal void UpdateUsers(List<User> users)
    {
        _userCacheDict.Clear();
        foreach (var user in users) _userCacheDict.TryAdd(user.UID, user);
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
}