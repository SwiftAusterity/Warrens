using NetMud.DataStructure.Base.EntityBackingData;
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
    public class PlayerDataCacheKey : ICacheKey
    {
        /// <summary>
        /// The type of cache this is used for
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public CacheType CacheType
        {
            get { return CacheType.PlayerData; }
        }

        /// <summary>
        /// System type of the object being cached
        /// </summary>
        public Type ObjectType { get; set; }

        /// <summary>
        /// The account handle
        /// </summary>
        public string AccountHandle { get; set; }

        /// <summary>
        /// The character object's Id
        /// </summary>
        public long CharacterId { get; set; }

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
        /// Make a new cache key using the object
        /// </summary>
        /// <param name="character">the character object</param>
        public PlayerDataCacheKey(ICharacter character)
        {
            ObjectType = character.GetType();
            AccountHandle = character.AccountHandle;
            CharacterId = character.Id;
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
