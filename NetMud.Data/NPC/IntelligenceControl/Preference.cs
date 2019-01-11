using NetMud.DataStructure.NPC.IntelligenceControl;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.NPC.IntelligenceControl
{
    /// <summary>
    /// A single preference pairing
    /// </summary>
    [Serializable]
    public class Preference : IPreference
    {
        /// <summary>
        /// What overall context does this preference apply to
        /// </summary>
        [Display(Name = "Context", Description = " What overall context does this preference apply to.")]
        [DataType(DataType.Text)]
        [UIHint("EnumDropdownList")]
        public PreferenceContext Context { get; set; }

        /// <summary>
        /// The quality this preference targets
        /// </summary>
        [Display(Name = "Preference", Description = "The quality this preference targets.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        /// <summary>
        /// The multiplier this adds to influence, can be negative
        /// </summary>
        [Display(Name = "Modifier", Description = "The modifier this adds to influence, can be negative.")]
        [DataType(DataType.Text)]
        public int Multiplier { get; set; }
    }
}
