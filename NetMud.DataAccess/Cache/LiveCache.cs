using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public static class LiveCache
    {
        private static CacheAccessor BackingCache = new CacheAccessor(CacheType.Live);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add<T>(T objectToCache) where T : ILiveData
        {
            BackingCache.Add(objectToCache, new LiveCacheKey(objectToCache));
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
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetAll<T>()
        {
            return BackingCache.GetAll<T>();
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(HashSet<string> birthmarks) where T : ILiveData
        {
            return BackingCache.GetMany<T>(birthmarks);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<string> birthmarks) where T : ILiveData
        {
            return BackingCache.GetMany<T>(birthmarks);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="keys">the keys to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<LiveCacheKey> keys) where T : ILiveData
        {
            return BackingCache.GetMany<T>(keys);
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<ILiveData> GetAll()
        {
            return BackingCache.GetAll<ILiveData>();
        }

        /// <summary>
        /// When base type and maintype want to be less ambigious
        /// </summary>
        /// <typeparam name="T">The base type (like ILocation)</typeparam>
        /// <param name="mainType">The inheriting type (like IRoom)</param>
        /// <returns>all the stuff and things</returns>
        public static IEnumerable<T> GetAll<T>(Type mainType)
        {
            return BackingCache.GetAll<T>(mainType);
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
        public static T Get<T>(LiveCacheKey key) where T : ILiveData
        {
            return BackingCache.Get<T>(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity (as an object) requested</returns>
        public static object Get(LiveCacheKey key)
        {
            return BackingCache.Get(key);
        }

        /// <summary>
        /// Gets one entity from the cache by its Id, only works for Singleton spawners with data templates(IEntities)
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(long id) where T : IEntity
        {
            try
            {
                IEnumerable<T> dataCluster = GetAll<T>();

                if (dataCluster.Any(p => ((IEntity)p).TemplateId.Equals(id)))
                {
                    return dataCluster.First(p => ((IEntity)p).TemplateId.Equals(id));
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Gets one entity from the cache by its Id, only works for Singleton spawners
        /// </summary>
        /// <typeparam name="T">the underlying type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <param name="mainType">the primary type of the entity</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(long id, Type backingDataType) where T : IEntity
        {
            try
            {
                ITemplate backingData = Activator.CreateInstance(backingDataType) as ITemplate;

                IEnumerable<T> dataCluster = GetAll<T>().Where(thing => thing.GetType() == backingData.EntityClass);

                if (dataCluster.Any(p => ((IEntity)p).TemplateId.Equals(id)))
                {
                    return dataCluster.First(p => ((IEntity)p).TemplateId.Equals(id));
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(LiveCacheKey key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Removes an non-entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(string key)
        {
            BackingCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(LiveCacheKey key)
        {
            return BackingCache.Exists(key);
        }


        /// <summary>
        /// Checks if an non-entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(string key)
        {
            return BackingCache.Exists(key);
        }

        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetUniqueIdentifier(IEntity obj)
        {
            return GetUniqueIdentifier(obj.TemplateId.ToString());
        }

        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetUniqueIdentifier(IKeyedData obj)
        {
            return GetUniqueIdentifier(obj.Id.ToString());
        }

        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetUniqueIdentifier(string marker)
        {
            return string.Format("{0}.{1}.{2}", marker, DateTime.Now.ToBinary(), Guid.NewGuid().ToString().Substring(30));
        }
    }
}
