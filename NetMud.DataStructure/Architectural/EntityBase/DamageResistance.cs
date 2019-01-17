using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Damage resistance rating
    /// </summary>
    [Serializable]
    public class DamageResistance
    {
        /// <summary>
        /// The type of damage this is for
        /// </summary>
        [Display(Name = "Type", Description = "The damage type for this resistance.")]
        [UIHint("EnumDropDownList")]
        public DamageType Type { get; set; }

        /// <summary>
        /// The percentage (-100 to 100) of resistance
        /// </summary>
        [Display(Name = "Resistance", Description = "How resistant (1 to 100) this is to the damage type.")]
        [DataType(DataType.Text)]
        public short Resistance { get; set; }
    }
}
