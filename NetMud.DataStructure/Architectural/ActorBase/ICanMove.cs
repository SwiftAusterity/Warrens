namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// For mobiles, can move through pathways
    /// </summary>
    public interface ICanMove
    {
        /// <summary>
        /// Current stance this is in
        /// </summary>
        MobilityState CurrentPosition { get; set; }

        /// <summary>
        /// How stunned are you (for stun state resistance)
        /// </summary>
        int StunStatus { get; set; }

        /// <summary>
        /// How rooted are you (for root stack resistance)
        /// </summary>
        int RootStatus { get; set; }
    }
}
