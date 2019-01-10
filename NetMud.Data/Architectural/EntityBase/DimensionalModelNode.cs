using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Single node for a dimensional model
    /// </summary>
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
        private TemplateCacheKey _compositionId { get; set; }

        /// <summary>
        /// Material composition of the node
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IMaterial Composition 
        { 
            get
            {
                return TemplateCache.Get<IMaterial>(_compositionId);
            }
            set
            {
                _compositionId = new TemplateCacheKey(value);
            }
        }
    }
}
