namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates an entity loses durability over time
    /// </summary>
    public interface IDecay
    {
        /// <summary>
        /// How much of this is left
        /// </summary>
        int Integrity { get; set; }
    }
}
