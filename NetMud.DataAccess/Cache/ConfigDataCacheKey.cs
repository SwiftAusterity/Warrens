using NetMud.DataStructure.Architectural;
using Newtonsoft.Json;
using System;

namespace NetMud.DataAccess.Cache
{
    /// <summary>
    /// A cache key for live entities
    /// </summary>
    [Serializable]
    public class ConfigDataCacheKey : ICacheKey
    {
        [JsonIgnore]

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
        public ConfigDataCacheKey(Type objectType, string uniqueKey, ConfigDataType type)
        {
            ObjectType = objectType;
            BirthMark = string.Format("{0}_{1}", type, uniqueKey);
        }

        /// <summary>
        /// Hash key used by the cache system
        /// </summary>
        /// <returns>the key's hash</returns>
        public string KeyHash()
        {
            string typeName = ObjectType.Name;

            //Normalize interfaces versus classnames
            if (ObjectType.IsInterface)
            {
                typeName = typeName.Substring(1);
            }

            return string.Format("{0}_{1}_{2}", CacheType.ToString(), typeName, BirthMark.ToString());
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ICacheKey other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.KeyHash().Equals(KeyHash()))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ICacheKey other)
        {
            if (other != default(ICacheKey))
            {
                try
                {
                    return other.GetType() == GetType() && other.KeyHash().Equals(KeyHash());
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ICacheKey x, ICacheKey y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ICacheKey obj)
        {
            return obj.GetType().GetHashCode() + obj.KeyHash().GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + KeyHash().GetHashCode();
        }
        #endregion
    }
}
