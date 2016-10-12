using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    [Serializable]
    public class DimensionalModelNode : IDimensionalModelNode
    {
        /// <summary>
        /// The position of this node on the XAxis
        /// </summary>
        public short XAxis { get; set; }

        /// <summary>
        /// The Y-axis from the plane this belongs to
        /// </summary>
        public short YAxis { get; set; }

        /// <summary>
        /// The damage type inflicted when this part of the model strikes
        /// </summary>
        public DamageType Style { get; set; }

        [JsonProperty("CompositionId")]
        private long _compositionId { get; set; }

        /// <summary>
        /// Material composition of the node
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IMaterial Composition 
        { 
            get
            {
                return BackingDataCache.Get<IMaterial>(_compositionId);
            }
            set
            {
                _compositionId = value.ID;
            }
        }
    }
}
