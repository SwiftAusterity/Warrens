using System;
using System.Collections.Generic;

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
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        IList<string> FitnessReport();

        /// <summary>
        /// Does this data have fitness problems?
        /// </summary>
        bool FitnessProblems { get; }

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
