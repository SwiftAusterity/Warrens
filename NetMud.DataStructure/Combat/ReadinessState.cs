namespace NetMud.DataStructure.Combat
{
    /// <summary>
    /// State of combat readiness for an art
    /// </summary>
    public enum ReadinessState
    {
        /// <summary>
        /// General state
        /// </summary>
        Offensive,
        /// <summary>
        /// Dodge readiness state
        /// </summary>
        Circle,
        /// <summary>
        /// Half dodge/block, mitigates damage and impact and leaves you close
        /// </summary>
        Deflect,
        /// <summary>
        /// Full block, mitigates damage and impact
        /// </summary>
        Block,
        /// <summary>
        /// redirect attack back at attacker. difficult
        /// </summary>
        Redirect
    }
}
