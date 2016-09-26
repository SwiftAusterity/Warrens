using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
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
        private static CacheAccessor BackingCache = new CacheAccessor(CacheType.PlayerData);

        /// <summary>
        /// Adds a single entity into the cache
        /// </summary>
        /// <param name="objectToCache">the entity to cache</param>
        public static void Add(ICharacter objectToCache)
        {
            var cacheKey = new PlayerDataCacheKey(typeof(ICharacter), objectToCache.AccountHandle, objectToCache.ID);

            BackingCache.Add(objectToCache, cacheKey);
        }

        /// <summary>
        /// Get all entities of a type from the cache
        /// </summary>
        /// <typeparam name="T">the system type for the entity</typeparam>
        /// <returns>a list of the entities from the cache</returns>
        public static IEnumerable<ICharacter> GetAllForAccountHandle(string accountHandle)
        {
            EnsureAccountCharacters(accountHandle);

            return BackingCache.GetAll<ICharacter>().Where(ch => ch.AccountHandle.Equals(accountHandle, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Only for the hotbackup procedure
        /// </summary>
        /// <returns>All entities in the entire system</returns>
        public static IEnumerable<ICharacter> GetAll()
        {
            return BackingCache.GetAll<ICharacter>();
        }

        /// <summary>
        /// Gets one entity from the cache by its key
        /// </summary>
        /// <typeparam name="T">the type of the entity</typeparam>
        /// <param name="key">the key it was cached with</param>
        /// <returns>the entity requested</returns>
        public static ICharacter Get(PlayerDataCacheKey key)
        {
            EnsureAccountCharacters(key.AccountHandle);

            return BackingCache.Get<ICharacter>(key);
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

        private static void EnsureAccountCharacters(string accountHandle)
        {
            //No shenanigans
            if(String.IsNullOrWhiteSpace(accountHandle))
                return;

            var chars = GetAllForAccountHandle(accountHandle);

            if(!chars.Any())
            {
                var pData = new PlayerData();

                pData.LoadAllCharactersForAccountToCache(accountHandle);
            }
        }
    }

    /// <summary>
    /// A cache key for live entities
    /// </summary>
    public class PlayerDataCacheKey : ICacheKey
    {
        /// <summary>
        /// The account handle
        /// </summary>
        public string AccountHandle { get; set; }

        /// <summary>
        /// The character object's ID
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// The type of cache this is used for
        /// </summary>
        public CacheType CacheType
        {
            get { return CacheType.PlayerData; }
        }

        /// <summary>
        /// System type of the object being cached
        /// </summary>
        public Type ObjectType { get; set; }

        /// <summary>
        /// Unique signature for a live object
        /// </summary>
        public string BirthMark 
        { 
            get
            {
                return String.Format("{0}_{1}", AccountHandle, CharacterId);
            }
        }

        /// <summary>
        /// Generate a live key for a live object
        /// </summary>
        /// <param name="objectType">System type of the entity being cached</param>
        /// <param name="marker">Unique signature for a live entity</param>
        public PlayerDataCacheKey(Type objectType, string accountHandle, long characterId)
        {
            ObjectType = objectType;
            AccountHandle = accountHandle;
            CharacterId = characterId;
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            var typeName = ObjectType.Name;

            //Normalize interfaces versus classnames
            if (ObjectType.IsInterface)
                typeName = typeName.Substring(1);

            return string.Format("{0}_{1}_{2}", CacheType.ToString(), typeName, BirthMark.ToString());
        }
    }
}
