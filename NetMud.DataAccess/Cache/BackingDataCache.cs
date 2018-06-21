using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public static class BackingDataCache
    {
        private static CacheAccessor BackingCache = new CacheAccessor(CacheType.BackingData);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add<T>(T objectToCache) where T : IKeyedData
        {
            BackingCache.Add(objectToCache, new BackingDataCacheKey(objectToCache));
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
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<long> ids) where T : IKeyedData
        {
            return BackingCache.GetMany<T>(ids);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="keys">the keys to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<BackingDataCacheKey> keys) where T : IKeyedData
        {
            return BackingCache.GetMany<T>(keys);
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetAll<T>(bool onlyApproved = false)
        {
            //Don't waste the time with the where if it's false
            if (onlyApproved)
                return BackingCache.GetAll<T>().Where(data => ((IKeyedData)data).Approved);

            return BackingCache.GetAll<T>();
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<IKeyedData> GetAll(bool onlyApproved = false)
        {
            //Don't waste the time with the where if it's false
            if(onlyApproved)
                return BackingCache.GetAll<IKeyedData>().Where(data => data.Approved);

            return BackingCache.GetAll<IKeyedData>();
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T GetByName<T>(string name) where T : IKeyedData
        {
            var cacheItems = BackingCache.GetAll<T>();

            return cacheItems.FirstOrDefault<T>(ci => ci.Name.ToLower().Contains(name.ToLower()));
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T GetByKeywords<T>(string word) where T : IEntityBackingData
        {
            var cacheItems = BackingCache.GetAll<T>();

            return cacheItems.FirstOrDefault(ci => ci.Keywords.Contains(word.ToLower()));
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(string key)
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(BackingDataCacheKey key) where T : IKeyedData
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static object Get(BackingDataCacheKey key)
        {
            return BackingCache.Get(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its Id, only works for Singleton spawners with data templates(IEntities)
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(long id) where T : IKeyedData
        {
            var key = new BackingDataCacheKey(typeof(T), id);

            return Get<T>(key);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(BackingDataCacheKey key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(BackingDataCacheKey key)
        {
            return BackingCache.Exists(key);
        }
    }
}
