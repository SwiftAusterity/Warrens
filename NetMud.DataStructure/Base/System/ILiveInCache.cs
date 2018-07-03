namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Identifies this can be cached
    /// </summary>
    public interface ILiveInCache
    {
        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        bool PersistToCache();

        /// <summary>
        /// What type of cache is this using
        /// </summary>
        CacheType CachingType { get; }
    }
}
