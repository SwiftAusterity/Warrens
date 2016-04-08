using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Storage and access for live entities in game (including players)
    /// </summary>
    public static class LiveCache
    {
        /* 
         * The general idea here is that we are literally caching everything possible from app start.
         * 
         * We'll need to cache collections of references to things and the things themselves.
         * Caching collections of things will result in flipping the cache constantly
         * 
         * The administrative website will edit the reference data in the database which wont get refreshed
         * until someone tells it to (or the entire thing reboots)
         * 
         * IEntity data is ALWAYS cached and saved to a different place because it is live in-game data and even if
         * we add, say, one damage to the Combat Knife item in the db it doesn't mean all Combat Knife objects in game
         * get retroactively updated. There will be superadmin level website commands to do this and in-game commands for admins.
         * 
         */

        /// <summary>
        /// The place everything gets stored
        /// </summary>
        private static ObjectCache globalCache = MemoryCache.Default;
        /// <summary>
        /// The general storage policy
        /// </summary>
        private static CacheItemPolicy globalPolicy = new CacheItemPolicy();

        /// <summary>
        /// Dumps everything of a single type into the cache from the database for BackingData
        /// </summary>
        /// <typeparam name="T">the type to get and store</typeparam>
        /// <returns>success status</returns>
        public static bool PreLoadAll<T>() where T : IData
        {
            var backingClass = Activator.CreateInstance(typeof(T)) as IEntityBackingData;

            var implimentingEntityClass = backingClass.EntityClass;

            foreach (IData thing in DataWrapper.GetAll<T>())
            {
                var entityThing = Activator.CreateInstance(implimentingEntityClass, new object[] { (T)thing }) as IEntity;

                var cacheKey = new LiveCacheKey(implimentingEntityClass, entityThing.BirthMark);

                globalCache.AddOrGetExisting(cacheKey.KeyHash(), entityThing, globalPolicy);
            }

            return true;
        }

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add(object objectToCache)
        {
            var entityToCache = (IEntity)objectToCache;
            var cacheKey = new LiveCacheKey(objectToCache.GetType(), entityToCache.BirthMark);

            if (!globalCache.Contains(cacheKey.KeyHash()))
                globalCache.AddOrGetExisting(cacheKey.KeyHash(), objectToCache, globalPolicy);
            else
            {
                globalCache.Remove(cacheKey.KeyHash());
                globalCache.Add(cacheKey.KeyHash(), objectToCache, globalPolicy);
            }
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

    }

    /// <summary>
    /// A cache key for live entities
    /// </summary>
    public class LiveCacheKey
    {
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
            return string.Format("{0}", BirthMark.ToString());
        }
    }

    /// <summary>
    /// Unique signature for a live entity
    /// </summary>
    public static class Birthmarker
    {
        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetBirthmark(IData obj)
        {
            return string.Format("{0}.{1}.{2}", obj.ID, DateTime.Now.ToBinary(), Guid.NewGuid().ToString().Replace("-", string.Empty));
        }
    }
}
