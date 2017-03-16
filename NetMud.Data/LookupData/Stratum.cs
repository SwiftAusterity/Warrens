using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    [Serializable]
    public class Stratum : BackingDataPartial, IStratum
    {
        /// <summary>
        /// Diameter of this stratum in meters
        /// </summary>
        public long Diameter { get; set; }

        /// <summary>
        /// What the various layers of the strata are typically composed of
        /// </summary>
        public Dictionary<string, IStratumLayer> Layers { get; set; }

        /// <summary>
        /// How hot it is in this stratum generally
        /// </summary>
        public Tuple<int, int> AmbientTemperatureRange { get; set; }

        /// <summary>
        /// How humid it is in this stratum generally
        /// </summary>
        public Tuple<int, int> AmbientHumidityRange { get; set; }

        public Stratum()
        {
            Layers = new Dictionary<string, IStratumLayer>();
        }
    }

    /// <summary>
    /// Defines layers of strata
    /// </summary>
    [Serializable]
    public class StratumLayer : IStratumLayer
    {

        [JsonProperty("BaseMaterial")]
        private long _baseMaterial { get; set; }

        /// <summary>
        /// Material for this layer, can be null for Air layer
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMaterial BaseMaterial
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_baseMaterial);
            }
            set
            {
                _baseMaterial = value.ID;
            }
        }

        /// <summary>
        /// Lower Z bound for this layer
        /// </summary>
        public long LowerDepth { get; set; }

        /// <summary>
        /// Upper Z bound for this layer
        /// </summary>
        public long UpperDepth { get; set; }
    }
}
