using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Framework for Database objects
    /// </summary>
    public interface IData : IFileStored, ILiveInCache
    {
        /// <summary>
        /// Remove this entry perma
        /// </summary>
        /// <returns>success status</returns>
        bool Remove(IAccount remover, StaffRank creatorRank);

        /// <summary>
        /// Update this entry
        /// </summary>
        /// <returns>success status</returns>
        bool Save(IAccount editor, StaffRank creatorRank);

        /// <summary>
        /// Update this entry by the system
        /// </summary>
        /// <returns>success status</returns>
        bool SystemSave();
    }
}
