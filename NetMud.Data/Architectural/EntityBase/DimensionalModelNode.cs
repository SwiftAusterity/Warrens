using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "X-Axis", Description = "The x-axis of this node.")]
        [DataType(DataType.Text)]
        public short XAxis { get; set; }

        /// <summary>
        /// The Y-axis from the plane this belongs to
        /// </summary>
        [Display(Name = "Y-Axis", Description = "The y-axis of this node.")]
        [DataType(DataType.Text)]
        public short YAxis { get; set; }

        /// <summary>
        /// The damage type inflicted when this part of the model strikes
        /// </summary>
        [Display(Name = "Surface", Description = "The style of the surface of this node.")]
        [UIHint("EnumDropDownList")]
        public DamageType Style { get; set; }

        [JsonProperty("CompositionId")]
        private TemplateCacheKey _compositionId { get; set; }

        /// <summary>
        /// Material composition of the node
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Composition", Description = "The material this part is made of.")]
        [UIHint("MaterialList")]
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
