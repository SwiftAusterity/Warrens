using NetMud.Data.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Animal spawns
    /// </summary>
    [Serializable]
    public class Fauna : NaturalResourceDataPartial, IFauna
    {
        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        [IntDataIntegrity("Female to male ratio must be greater than 0.", 0)]
        public int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        [IntDataIntegrity("Population Hard Cap must be greater than 0.", 0)]
        public int PopulationHardCap { get; set; }

        [JsonProperty("Race")]
        private BackingDataCacheKey _race { get; set; }

        /// <summary>
        /// What we're spawning
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Race must be set.")]
        public IRace Race
        {
            get
            {
                return BackingDataCache.Get<IRace>(_race);
            }
            set
            {
                _race = new BackingDataCacheKey(value);
            }
        }
    }
}
