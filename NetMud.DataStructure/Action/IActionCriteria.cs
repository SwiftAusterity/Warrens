using NetMud.DataStructure.Architectural;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Action
{
    /// <summary>
    /// Pre-requisites to using an action
    /// </summary>
    public interface IActionCriteria : ICloneable
    {
        /// <summary>
        /// Target type of the criteria, what are we checking against
        /// </summary>
        [Display(Name = "Target Type", Description = "The types of things (tiles, NPCs, players, items) this can affect and target.")]
        [UIHint("SimpleDropdown")]
        ActionTarget Target { get; set; }

        /// <summary>
        /// Cheaty way of doing this - only affects entities with a backingdata of this ID, the actionTarget tells us the type
        /// </summary>
        [UIHint("KeyedDataList")]
        long AffectsMemberId { get; set; }

        /// <summary>
        /// The quality we're checking for
        /// </summary>

        [Display(Name = "Quality", Description = "Name of quality required before this is usable.")]
        [DataType(DataType.Text)]
        string Quality { get; set; }

        /// <summary>
        /// The value range of the quality we're checking for
        /// </summary>

        [Display(Name = "Value Cap", Description = "Value range for the high and low caps for the required quality.")]
        [UIHint("ValueRangeInt")]
        ValueRange<int> ValueRange { get; set; }

        /// <summary>
        /// Get the keyed data member out from the ID
        /// </summary>
        /// <typeparam name="T">The expected type of the member</typeparam>
        /// <returns>the member</returns>
        T GetMember<T>() where T : IKeyedData;
    }
}
