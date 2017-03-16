using NetMud.Data.System;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Composes the various horizontal layers of worlds
    /// </summary>
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
            //empty constructor
        }

        public Stratum(int lowTemp, int highTemp, int lowHumidity, int highHumidity, long diameter)
        {
            AmbientHumidityRange = new Tuple<int, int>(lowHumidity, highHumidity);
            AmbientTemperatureRange = new Tuple<int, int>(lowTemp, highTemp);
            Diameter = diameter;
        }
    }

    /// <summary>
    /// Defines layers of strata
    /// </summary>
    [Serializable]
    public class StratumLayer : IStratumLayer
    {
        /// <summary>
        /// Material for this layer, can be null for Air layer
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IMaterial BaseMaterial
        {
            get
            {
                if (_baseMaterial > -1)
                    return BackingDataCache.Get<IMaterial>(_baseMaterial);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _baseMaterial = value.ID;
            }
        }


        [JsonProperty("BaseMaterial")]
        private long _baseMaterial { get; set; }

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
