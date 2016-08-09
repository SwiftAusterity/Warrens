using System;
using System.Data;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Framework for Database objects
    /// </summary>
    public interface IData : IFileStored, IComparable<IData>, IEquatable<IData>
    {
        /// <summary>
        /// Unique, iterative ID for this entry
        /// </summary>
        long ID { get; set; }

        /// <summary>
        /// When this entry was first created
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// The last time this entry was revised
        /// </summary>
        DateTime LastRevised { get; set; }

        /// <summary>
        /// The name/keyword for this entry
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Fill this data object from a datarow
        /// </summary>
        /// <param name="dr">the datarow</param>
        void Fill(DataRow dr);

        /// <summary>
        /// Create a new db entry
        /// </summary>
        /// <returns>the new, filled db object</returns>
        IData Create();

        /// <summary>
        /// Remove this entry from the database permenantly
        /// </summary>
        /// <returns>success status</returns>
        bool Remove();

        /// <summary>
        /// Update this entry to the db
        /// </summary>
        /// <returns></returns>
        bool Save();
    }
}
