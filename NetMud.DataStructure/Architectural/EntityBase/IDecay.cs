namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates an entity loses durability over time
    /// </summary>
    public interface IDecay
    {
        int Integrity { get; set; }
    }
}
