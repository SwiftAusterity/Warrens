using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.NPCs
{
    /// <summary>
    /// Criteria for buying and selling merchandise
    /// </summary>
    [Serializable]
    public class Merchandise : IMerchandise
    {
        [JsonProperty("Item")]
        private TemplateCacheKey _item { get; set; }

        /// <summary>
        /// Item type
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate Item
        {
            get
            {
                if (_item == null)
                    return null;

                return TemplateCache.Get<IInanimateTemplate>(_item);
            }
            set
            {
                if (value != null)
                    _item = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Required quality
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Range for the quality
        /// </summary>
        public ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Markup or discount for buying/selling. 1 would be no markup/discount, below 1 would be discount
        /// </summary>
        public decimal MarkRate { get; set; }
    }
}
