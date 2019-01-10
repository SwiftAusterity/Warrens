using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Player;
using System;

namespace NetMud.DataStructure.Architectural
{
    /// <summary>
    /// Framework for Database objects
    /// </summary>
    public interface IData : IFileStored, ILiveInCache, ICloneable
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

        /// <summary>
        /// Remove this entry
        /// </summary>
        /// <returns>success status</returns>
        bool SystemRemove();
    }
}
