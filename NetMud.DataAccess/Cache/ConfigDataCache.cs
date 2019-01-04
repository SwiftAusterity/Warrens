using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public static class ConfigDataCache
    {
        private static CacheAccessor BackingCache = new CacheAccessor(CacheType.ConfigData);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add<T>(T objectToCache) where T : IConfigData
        {
            BackingCache.Add(objectToCache, new ConfigDataCacheKey(objectToCache));
        }

        /// <summary>
        /// Adds a non-entity to the cache
        /// </summary>
        /// <param name="objectToCache">the object to cache</param>
        /// <param name="cacheKey">the string key to cache it under</param>
        public static void Add(object objectToCache, string cacheKey)
        {
            BackingCache.Add(objectToCache, cacheKey);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="keys">the keys to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<ConfigDataCacheKey> keys) where T : IConfigData
        {
            return BackingCache.GetMany<T>(keys);
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetAll<T>()
        {
            return BackingCache.GetAll<T>();
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<IKeyedData> GetAll()
        {
            return BackingCache.GetAll<IKeyedData>();
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(ConfigDataCacheKey key) where T : IConfigData
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static object Get(ConfigDataCacheKey key)
        {
            return BackingCache.Get(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its Id, only works for Singleton spawners with data templates(IEntities)
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(string uniqueKey) where T : IConfigData
        {
            ConfigDataCacheKey key = new ConfigDataCacheKey(typeof(T), uniqueKey);

            return Get<T>(key);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(ConfigDataCacheKey key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(ConfigDataCacheKey key)
        {
            return BackingCache.Exists(key);
        }
    }
}
