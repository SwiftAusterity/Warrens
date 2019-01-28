using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.NPC
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
        [Display(Name = "Item", Description = "The item in question.")]
        [UIHint("InanimateTemplateList")]
        [Required]
        public IInanimateTemplate Item
        {
            get
            {
                if (_item == null)
                {
                    return null;
                }

                return TemplateCache.Get<IInanimateTemplate>(_item);
            }
            set
            {
                if (value != null)
                {
                    _item = new TemplateCacheKey(value);
                }
            }
        }

        /// <summary>
        /// Required quality
        /// </summary>
        [Display(Name = "Quality", Description = "The required quality for the item type.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        /// <summary>
        /// Range for the quality
        /// </summary>
        [Display(Name = "Quality Range", Description = "The value for the required quality.")]
        [UIHint("ValueRangeInt")]
        [IntValueRangeValidator(Optional = true)]
        public ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Markup or discount for buying/selling. 1 would be no markup/discount, below 1 would be discount
        /// </summary>
        [Display(Name = "Mark-rate", Description = "The markup (above 1) or discount (below 1) to apply to buying or selling this item.")]
        [DataType(DataType.Text)]
        public decimal MarkRate { get; set; }
    }
}
