using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.PropertyValidation;
using NetMud.DataStructure.Inanimate;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Criteria for buying and selling merchandise
    /// </summary>
    public interface IMerchandise
    {
        /// <summary>
        /// Item type
        /// </summary>
        [Display(Name = "Item", Description = "The item in question.")]
        [UIHint("InanimateTemplateList")]
        [Required]
        IInanimateTemplate Item { get; set; }

        /// <summary>
        /// Required quality
        /// </summary>
        [Display(Name = "Quality", Description = "The required quality for the item type.")]
        [DataType(DataType.Text)]
        string Quality { get; set; }

        /// <summary>
        /// Range for the quality
        /// </summary>
        [Display(Name = "Quality Range", Description = "The value for the required quality.")]
        [UIHint("ValueRangeInt")]
        [IntValueRangeValidator(Optional = true)]
        ValueRange<int> QualityRange { get; set; }

        /// <summary>
        /// Markup or discount for buying/selling. 1 would be no markup/discount, below 1 would be discount
        /// </summary>
        [Display(Name = "Mark-rate", Description = "The markup (above 1) or discount (below 1) to apply to buying or selling this item.")]
        [DataType(DataType.Text)]
        decimal MarkRate { get; set; }
    }
}
