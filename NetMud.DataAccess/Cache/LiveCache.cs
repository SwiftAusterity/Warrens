using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

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
        public static void Add<T>(T objectToCache) where T : IEntity
        {
            var entityToCache = (IEntity)objectToCache;
            var cacheKey = new LiveCacheKey(objectToCache.GetType(), entityToCache.BirthMark);

            BackingCache.Add(objectToCache, cacheKey);
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
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType() == typeof(T)
                                                    || (typeof(T).IsInterface && keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)))
                                    ).Select(kvp => (T)kvp.Value);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(HashSet<string> birthmarks) where T : IEntity
        {
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && birthmarks.Contains(((T)keyValuePair.Value).BirthMark))
                              .Select(kvp => (T)kvp.Value);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<T> GetMany<T>(IEnumerable<string> birthmarks) where T : IEntity
        {
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && birthmarks.Contains(((T)keyValuePair.Value).BirthMark))
                              .Select(kvp => (T)kvp.Value);
        }


        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<IEntity> GetAll()
        {
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(IEntity))).Select(kvp => (IEntity)kvp.Value);
        }

        /// <summary>
        /// When base type and maintype want to be less ambigious
        /// </summary>
        /// <typeparam name="T">The base type (like ILocation)</typeparam>
        /// <param name="mainType">The inheriting type (like IRoom)</param>
        /// <returns>all the stuff and things</returns>
        public static IEnumerable<T> GetAll<T>(Type mainType)
        {
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces()
                .Contains(typeof(T)) && keyValuePair.Value.GetType() == mainType)
                .Select(kvp => (T)kvp.Value);
        }

        /// <summary>
        /// Gets all of a non-entity from the cache
        /// </summary>
        /// <returns>All entities of a type in the entire system</returns>
        public static IEnumerable<T> GetAllNonEntity<T>()
        {
            return globalCache.Where(ob => ob.Value.GetType() == typeof(T)).Select(kvp => (T)kvp.Value);
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(string key)
        {
            try
            {
                return (T)globalCache[key];
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(LiveCacheKey key) where T : IEntity
        {
            try
            {
                return (T)globalCache[key.KeyHash()];
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Gets one entity from the cache by its ID, only works for Singleton spawners
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(long id) where T : IEntity
        {
            try
            {
                var allPlayers = GetAll<T>();

                if (allPlayers.Any(p => ((IEntity)p).DataTemplate.ID.Equals(id)))
                    return allPlayers.First(p => ((IEntity)p).DataTemplate.ID.Equals(id));
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Gets one entity from the cache by its ID, only works for Singleton spawners
        /// </summary>
        /// <typeparam name="T">the underlying type of the entity</typeparam>
        /// <param name="id">the id</param>
        /// <param name="mainType">the primary type of the entity</param>
        /// <returns>the entity requested</returns>
        public static T Get<T>(long id, Type mainType) where T : IEntity
        {
            try
            {
                var allTheStuff = GetAll<T>(mainType);

                if (allTheStuff.Any(p => ((IEntity)p).DataTemplate.ID.Equals(id)))
                    return allTheStuff.First(p => ((IEntity)p).DataTemplate.ID.Equals(id));
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
            globalCache.Remove(key.KeyHash());
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(LiveCacheKey key)
        {
            return globalCache.Get(key.KeyHash()) != null;
        }

        /// <summary>
        /// Removes an non-entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public static void Remove(string key)
        {
            globalCache.Remove(key);
        }

        /// <summary>
        /// Checks if an non-entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public static bool Exists(string key)
        {
            return globalCache.Get(key) != null;
        }

        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetUniqueIdentifier(object obj)
        {
            var dataObject = obj as IData;

            return string.Format("{0}.{1}.{2}", dataObject.ID, DateTime.Now.ToBinary(), Guid.NewGuid().ToString().Replace("-", string.Empty));
        }
    }

    /// <summary>
    /// A cache key for live entities
    /// </summary>
    public class LiveCacheKey : ICacheKey
    {
        public CacheType CacheType
        {
            get { return CacheType.Live; }
        }

        /// <summary>
        /// System type of the entity being cached
        /// </summary>
        public Type ObjectType { get; set; }

        /// <summary>
        /// Unique signature for a live entity
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// Generate a live key for an entity
        /// </summary>
        /// <param name="objectType">System type of the entity being cached</param>
        /// <param name="marker">Unique signature for a live entity</param>
        public LiveCacheKey(Type objectType, string marker)
        {
            ObjectType = objectType;
            BirthMark = marker;
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            //Not using type name right now, birthmarks are unique globally anyways
            return string.Format("{0}_{1}", CacheType.ToString(), BirthMark.ToString());
        }
    }
}
