using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataAccess
{
    public class LiveCache
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
        private ObjectCache globalCache;
        private CacheItemPolicy globalPolicy;

        public LiveCache()
        {
            globalCache = MemoryCache.Default;
            globalPolicy = new CacheItemPolicy();
        }

        public T Add<T>(T objectToCache) where T : IEntity
        {
            var cacheKey = new LiveCacheKey(typeof(T), objectToCache.BirthMark);

           return (T)globalCache.AddOrGetExisting(cacheKey.KeyHash(), objectToCache, globalPolicy);
        }

        public IEnumerable<T> GetAll<T>()
        {
            return globalCache.Where(keyValuePair => keyValuePair.Value.GetType() == typeof(T)).Select(kvp => (T)kvp.Value);
        }

        public T Get<T>(LiveCacheKey key) where T : IEntity
        {
            try
            {
                return (T)globalCache[key.KeyHash()];
            }
            catch
            {
                //TODO: Logging, why were we looking for something that didn't exist?
            }

            return default(T);
        }

        public void Remove(LiveCacheKey key)
        {
            globalCache.Remove(key.KeyHash());
        }

        public bool Exists(LiveCacheKey key)
        {
            return globalCache.Get(key.KeyHash()) != null;
        }

    }

    public class LiveCacheKey
    {
        public Type ObjectType { get; set; }
        public String BirthMark { get; set; }

        public LiveCacheKey(Type objectType, string marker)
        {
            ObjectType = objectType;
            BirthMark = marker;
        }

        public string KeyHash()
        {
            return String.Format("{0}.{1}", ObjectType.Name, BirthMark.ToString());
        }
    }

    public static class Birthmarker
    {
        /// <summary>
        /// Gets birthmarks for live entities
        /// </summary>
        /// <returns>the birthmark string</returns>
        public static string GetBirthmark()
        {
            return String.Format("{0}.{1}", DateTime.Now.ToBinary(), Guid.NewGuid().ToString().Replace("-", String.Empty));
        }
    }
}
