using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// A cache key for live entities
    /// </summary>
    [Serializable]
    public class LiveCacheKey : ICacheKey
    {
        [JsonIgnore]
        [ScriptIgnore]
        public CacheType CacheType
        {
            get { return CacheType.Live; }
        }

        /// <summary>
        /// System type of the object being cached
        /// </summary>
        public Type ObjectType { get; set; }

        /// <summary>
        /// Unique signature for a live object
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// Generate a live key for a live object
        /// </summary>
        /// <param name="objectType">System type of the entity being cached</param>
        /// <param name="marker">Unique signature for a live entity</param>
        [JsonConstructor]
        public LiveCacheKey(Type objectType, string birthMark)
        {
            ObjectType = objectType;
            BirthMark = birthMark;
        }

        /// <summary>
        /// Make a new cache key using the object
        /// </summary>
        /// <param name="data">the object</param>
        public LiveCacheKey(ILiveData data)
        {
            ObjectType = data.GetType();
            BirthMark = data.BirthMark;
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            //Not using type name right now, birthmarks are unique globally anyways
            return string.Format("{0}_{1}", CacheType.ToString(), BirthMark);
        }
    }
}
