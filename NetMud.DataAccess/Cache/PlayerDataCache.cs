using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// Storage and access for characters only, different than normal cache as it is on-demand load as opposed to front-loaded on start
    /// </summary>
    public static class PlayerDataCache
    {
        private static readonly CacheAccessor BackingCache = new(CacheType.PlayerData);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add(IPlayerTemplate objectToCache)
        {
            PlayerDataCacheKey cacheKey = new(objectToCache);

            BackingCache.Add(objectToCache, cacheKey);
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<IPlayerTemplate> GetAllForAccountHandle(string accountHandle)
        {
            return EnsureAccountCharacters(accountHandle);
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<IPlayerTemplate> GetAll()
        {
            return BackingCache.GetAll<IPlayerTemplate>();
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static IPlayerTemplate Get(PlayerDataCacheKey key)
        {
            EnsureAccountCharacters(key.AccountHandle);

            return BackingCache.Get<IPlayerTemplate>(key);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(PlayerDataCacheKey key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(PlayerDataCacheKey key)
        {
            return BackingCache.Exists(key);
        }

        private static IEnumerable<IPlayerTemplate> EnsureAccountCharacters(string accountHandle)
        {
            //No shenanigans
            if(string.IsNullOrWhiteSpace(accountHandle))
            {
                return Enumerable.Empty<IPlayerTemplate>();
            }

            IEnumerable<IPlayerTemplate> chars = GetAll().Where(ch => ch.AccountHandle.Equals(accountHandle, StringComparison.InvariantCultureIgnoreCase));

            if(!chars.Any())
            {
                PlayerData pData = new();

                pData.LoadAllCharactersForAccountToCache(accountHandle);
            }

            return chars;
        }
    }
}
