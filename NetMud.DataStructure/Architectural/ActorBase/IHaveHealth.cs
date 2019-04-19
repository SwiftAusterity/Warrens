namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// This has health (hp)
    /// </summary>
    public interface IHaveHealth
    {
        /// <summary>
        /// How much total health one has
        /// </summary>
        ulong TotalHealth { get; set; }
    }
}
