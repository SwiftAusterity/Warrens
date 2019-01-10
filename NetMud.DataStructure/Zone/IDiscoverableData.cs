namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Discoverability for backing data classes
    /// </summary>
    public interface IDiscoverableData
    {
        /// <summary>
        /// Does this even need to be discovered?
        /// </summary>
        bool AlwaysDiscovered { get; set; }
    }
}
