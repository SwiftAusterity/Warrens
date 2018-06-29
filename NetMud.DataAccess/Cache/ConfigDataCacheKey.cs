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
    public class ConfigDataCacheKey : ICacheKey
    {
        [JsonIgnore]
        [ScriptIgnore]
        public CacheType CacheType => CacheType.ConfigData;

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
        public ConfigDataCacheKey(Type objectType, string birthMark)
        {
            ObjectType = objectType;
            BirthMark = birthMark;
        }

        /// <summary>
        /// Make a new cache key using the object
        /// </summary>
        /// <param name="data">the object</param>
        public ConfigDataCacheKey(IConfigData data)
        {
            ObjectType = data.GetType();
            BirthMark  = string.Format("{0}_{1}", data.Type, data.UniqueKey);
        }

        /// <summary>
        /// Generate a live key for a live object
        /// </summary>
        /// <param name="objectType">System type of the entity being cached</param>
        /// <param name="marker">Unique signature for a live entity</param>
        [JsonConstructor]
        public ConfigDataCacheKey(Type objectType, string uniqueKey, ConfigDataType type)
        {
            ObjectType = objectType;
            BirthMark = string.Format("{0}_{1}", type, uniqueKey); ;
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
