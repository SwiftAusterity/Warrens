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
        PreferenceContext Context { get; set; }

        /// <summary>
        /// The quality this preference targets
        /// </summary>
        string Quality { get; set; }

        /// <summary>
        /// The multiplier this adds to influence, can be negative
        /// </summary>
        int Multiplier { get; set; }
    }
}
