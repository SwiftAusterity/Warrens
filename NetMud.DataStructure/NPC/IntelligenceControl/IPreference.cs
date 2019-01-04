using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    /// <summary>
    /// A single preference pairing
    /// </summary>
    public interface IPreference
    {
        /// <summary>
        /// What overall context does this preference apply to
        /// </summary>
        [Display(Name = "Context", Description = " What overall context does this preference apply to.")]
        [DataType(DataType.Text)]
        [UIHint("EnumDropdownList")]
        PreferenceContext Context { get; set; }

        /// <summary>
        /// The quality this preference targets
        /// </summary>
        [Display(Name = "Preference", Description = "The quality this preference targets.")]
        [DataType(DataType.Text)]
        string Quality { get; set; }

        /// <summary>
        /// The multiplier this adds to influence, can be negative
        /// </summary>
        [Display(Name = "Modifier", Description = "The modifier this adds to influence, can be negative.")]
        [DataType(DataType.Text)]
        int Multiplier { get; set; }
    }
}
