namespace NetMud.DataStructure.Base.Supporting
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
