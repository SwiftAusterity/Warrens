using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Tile;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Action
{
    /// <summary>
    /// Results of using an action
    /// </summary>
    public interface IActionResult : ICloneable
    {
        /// <summary>
        /// Target type of the criteria, what are we checking against
        /// </summary>
        [Display(Name = "Target Type", Description = "The types of things (tiles, NPCs, players, items) this can affect and target.")]
        [UIHint("SimpleDropdown")]
        ActionTarget Target { get; set; }

        /// <summary>
        /// What occurrence group is this a part of
        /// </summary>
        [Display(Name = "Occurrence Group", Description = "Is this result a random chance to occur within a clustered group of results. (with this same id)")]
        [DataType(DataType.Text)]
        short OccurrenceChanceGroupId { get; set; }

        /// <summary>
        /// Percentage modifier (1-100) of being chosen as the prize within the occurrence group
        /// </summary>
        [Display(Name = "Occurrence Group Chance", Description = "How likely is this result to be the one chosen within this occurrence group.")]
        [Range(typeof(decimal), "0.01", "100", ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        decimal OccurrenceChanceRate { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>
        [Display(Name = "Quality", Description = "Name of quality this adds to on use.")]
        [DataType(DataType.Text)]
        string Quality { get; set; }

        /// <summary>
        /// Is this quality additive or replace
        /// </summary>
        [Display(Name = "Additive", Description = "Does this quality add to an existing quality (true) or replace the value(false).")]
        [UIHint("Boolean")]
        bool AdditiveQuality { get; set; }

        /// <summary>
        /// The value we're adding to the quality
        /// </summary>
        [Display(Name = "Amount", Description = "How much is added for the applied quality.")]
        [DataType(DataType.Text)]
        int QualityValue { get; set; }

        /// <summary>
        /// Is the thing we're looking at consumed entirely
        /// </summary>
        [Display(Name = "Consumes", Description = "Does this delete the target when the action is used.")]
        [UIHint("Boolean")]
        bool Consumes { get; set; }

        /// <summary>
        /// Health damage done to the target
        /// </summary>
        [Display(Name = "Health Damage", Description = "How much stamina this take away from the target when used.")]
        [DataType(DataType.Text)]
        int HealthDamage { get; set; }

        /// <summary>
        /// Stamina damage done to the target
        /// </summary>
        [Display(Name = "Stamina Damage", Description = "How much stamina this take away from the target when used.")]
        [DataType(DataType.Text)]
        int StaminaDamage { get; set; }

        /// <summary>
        /// Does this produce items
        /// </summary>
        [Display(Name = "Produces", Description = "Any items this action will produce (such as creating Corn or Dirt).")]
        [UIHint("InanimateTemplateList")]
        IInanimateTemplate Produces { get; set; }

        /// <summary>
        /// Does this produce items to the actor's inventory
        /// </summary>
        [Display(Name = "Produces to Inventory", Description = "Does this produce its items to the actor's inventory directly.")]
        [UIHint("Boolean")]
        bool ProducesToInventory { get; set; }

        /// <summary>
        /// How many items does it produce
        /// </summary>
        [Display(Name = "Yield", Description = "How many items (from Produces) this yields when this action is taken.")]
        [DataType(DataType.Text)]
        int ProducesAmount { get; set; }

        /// <summary>
        /// The resulting tile change for tile targets
        /// </summary>
        [Display(Name = "Result", Description = "The new state the ground (tile) will be in after this action.")]
        [UIHint("BackingDataDropdown")]
        ITileTemplate Result { get; set; }
    }
}
