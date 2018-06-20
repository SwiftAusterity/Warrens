namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Framework for Database objects
    /// </summary>
    public interface IData : IFileStored
    {
        /// <summary>
        /// Remove this entry perma
        /// </summary>
        /// <returns>success status</returns>
        bool Remove();

        /// <summary>
        /// Update this entry
        /// </summary>
        /// <returns></returns>
        bool Save();
    }
}
