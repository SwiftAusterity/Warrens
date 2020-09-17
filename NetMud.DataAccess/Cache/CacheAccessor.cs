using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace NetMud.DataAccess.Cache
{
    internal class CacheAccessor
    {
        /* 
         * The general idea here is that we are literally caching everything possible from app start.
         * 
         * We'll need to cache collections of references to things and the things themselves.
         * Caching collections of things will result in flipping the cache constantly
         * 
         * The administrative website will edit the Lookup Data in the database which wont get refreshed
         * until someone tells it to (or the entire thing reboots)
         * 
         * ILiveData data is ALWAYS cached and saved to a different place because it is live in-game data and even if
         * we add, say, one damage to the Combat Knife item in the db it doesn't mean all Combat Knife objects in game
         * get retroactively updated. There will be superadmin level website commands to do this and in-game commands for admins.
         * 
         */

        /// <summary>
        /// The place everything gets stored
        /// </summary>
        private readonly ObjectCache _globalCache = MemoryCache.Default;

        /// <summary>
        /// The general storage policy
        /// </summary>
        private readonly CacheItemPolicy _globalPolicy = new CacheItemPolicy();

        /// <summary>
        /// Create a new CacheAccessor with its type
        /// </summary>
        /// <param name="type">The type of item we're caching</param>
        internal CacheAccessor()
        {
        }

        /// <summary>
        /// Adds an object to the cache
        /// </summary>
        /// <param name="objectToCache">the object to cache</param>
        /// <param name="cacheKey">the key to cache it under</param>
        public void Add(object objectToCache, ICacheKey cacheKey)
        {
            Add(objectToCache, cacheKey.KeyHash());
        }

        /// <summary>
        /// Adds an object to the cache
        /// </summary>
        /// <param name="objectToCache">the object to cache</param>
        /// <param name="cacheKey">the string key to cache it under</param>
        public void Add(object objectToCache, string cacheKey)
        {
            if (Exists(cacheKey))
            {
                Remove(cacheKey);
            }

            _globalCache.Add(cacheKey, objectToCache, _globalPolicy);
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="keys">the keys to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IQueryable<T> GetMany<T>(IEnumerable<ICacheKey> keys)
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && keys.Any(key => key.KeyHash().Equals(keyValuePair.Key)))
                                  .Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IQueryable<T> GetMany<T>(HashSet<string> birthmarks) where T : ILiveData
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && birthmarks.Contains(((T)keyValuePair.Value).BirthMark))
                                  .Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IQueryable<T> GetMany<T>(IEnumerable<string> birthmarks) where T : ILiveData
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && birthmarks.Contains(((T)keyValuePair.Value).BirthMark))
                                  .Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// fills a list of entities from the cache of a single type that match the birthmarks sent in
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <param name="birthmarks">the birthmarks to retrieve</param>
        /// <returns>a list of the entities from the cache</returns>
        public IQueryable<T> GetMany<T>(IEnumerable<long> ids) where T : IKeyedData
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T))
                                                        && ids.Contains(((T)keyValuePair.Value).Id))
                                  .Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public IQueryable<T> GetAll<T>()
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType() == typeof(T)
                                                        || (typeof(T).IsInterface && keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)))
                                        ).Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// When base type and maintype want to be less ambigious
        /// </summary>
        /// <typeparam name="T">The base type (like ILocation)</typeparam>
        /// <param name="mainType">The inheriting type (like IRoom)</param>
        /// <returns>all the stuff and things</returns>
        public IQueryable<T> GetAll<T>(Type mainType)
        {
            try
            {
                return _globalCache.AsQueryable().Where(keyValuePair => keyValuePair.Value.GetType().GetInterfaces().Contains(typeof(T)) && keyValuePair.Value.GetType() == mainType)
                        .Select(kvp => (T)kvp.Value);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// Gets one non-entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(string key)
        {
            try
            {
                return (T)_globalCache[key];
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default;
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public T Get<T>(ICacheKey key)
        {
            try
            {
                return Get<T>(key.KeyHash());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default;
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public object Get(ICacheKey key)
        {
            try
            {
                return _globalCache[key.KeyHash()];
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return null;
        }

        /// <summary>
        /// Removes an entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public void Remove(ICacheKey key)
        {
            try
            {
                Remove(key.KeyHash());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Removes an non-entity from the cache by its key
        /// </summary>
        /// <param name="key">the key of the entity to remove</param>
        public void Remove(string key)
        {
            _globalCache.Remove(key);
        }

        /// <summary>
        /// Checks if an entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public bool Exists(ICacheKey key)
        {
            return Exists(key.KeyHash());
        }

        /// <summary>
        /// Checks if an non-entity is in the cache
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>if it is in the cache of not</returns>
        public bool Exists(string key)
        {
            return _globalCache.Get(key) != null;
        }
    }
}
