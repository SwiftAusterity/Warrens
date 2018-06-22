using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Framework for ID Stored objects
    /// </summary>
    public interface IKeyedData : IData, INeedApproval, IComparable<IKeyedData>, IEquatable<IKeyedData>, IEqualityComparer<IKeyedData>
    {
        /// <summary>
        /// Unique, iterative Id for this entry
        /// </summary>
        long Id { get; set; }

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
        /// Create a new entry
        /// </summary>
        /// <returns>the new, filled db object</returns>
        IKeyedData Create(IAccount creator, StaffRank creatorRank);
    }
}
