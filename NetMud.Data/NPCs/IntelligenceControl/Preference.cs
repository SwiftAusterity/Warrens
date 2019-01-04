using NetMud.DataStructure.NPC.IntelligenceControl;
using System;

namespace NetMud.Data.NPCs.IntelligenceControl
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
        public PreferenceContext Context { get; set; }

        /// <summary>
        /// The quality this preference targets
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// The multiplier this adds to influence, can be negative
        /// </summary>
        public int Multiplier { get; set; }
    }
}
