namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Locations are special containers for entities
    /// </summary>
    public interface ILocation
    {
        /// <summary>
        /// current maximum section
        /// </summary>
        ulong MaxSection { get; set; }
    }
}
