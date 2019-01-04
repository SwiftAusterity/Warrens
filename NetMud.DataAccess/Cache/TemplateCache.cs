using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public static class TemplateCache
    {
        private static CacheAccessor BackingCache = new CacheAccessor(CacheType.Template);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add<T>(T objectToCache) where T : IKeyedData
        {
            BackingCache.Add(objectToCache, new TemplateCacheKey(objectToCache));
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
        public static IEnumerable<T> GetMany<T>(IEnumerable<TemplateCacheKey> keys) where T : IKeyedData
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
                return BackingCache.GetAll<T>().Where(data => ((IKeyedData)data).SuitableForUse);

            return BackingCache.GetAll<T>();
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetAllOfMine<T>(IGaiaTemplate ownerWorld) where T : IBelongToAWorld
        {
            if(ownerWorld == null)
                return BackingCache.GetAll<T>().Where(data => ((IKeyedData)data).SuitableForUse);

            return BackingCache.GetAll<T>().Where(data => ((IKeyedData)data).SuitableForUse 
                                                    && ((IBelongToAWorld)data).OwnerWorld != null 
                                                    && ((IBelongToAWorld)data).OwnerWorld.Id.Equals(ownerWorld.Id));
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<IKeyedData> GetAll(bool onlyApproved = false)
        {
            //Don't waste the time with the where if it's false
            if(onlyApproved)
                return BackingCache.GetAll<IKeyedData>().Where(data => data.SuitableForUse);

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
            IEnumerable<T> cacheItems = BackingCache.GetAll<T>();

            return cacheItems.FirstOrDefault(ci => ci.Name.ToLower().Contains(name.ToLower()));
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T GetByKeywords<T>(string word) where T : ITemplate
        {
            IEnumerable<T> cacheItems = BackingCache.GetAll<T>();

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
        public static T Get<T>(TemplateCacheKey key) where T : IKeyedData
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static object Get(TemplateCacheKey key)
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
            TemplateCacheKey key = new TemplateCacheKey(typeof(T), id);

            return Get<T>(key);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(TemplateCacheKey key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(TemplateCacheKey key)
        {
            return BackingCache.Exists(key);
        }
    }
}
