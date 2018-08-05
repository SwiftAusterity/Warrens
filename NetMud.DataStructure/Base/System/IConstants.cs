using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Constant values for use in the game code for localization purposes
    /// </summary>
    public interface IConstants : IFileStored, IKeyedData
    {
        /// <summary>
        /// All string values
        /// </summary>
        Dictionary<ILookupCriteria, HashSet<string>> Values { get; set; }

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new strings to add</param>
        void AddOrUpdate(ILookupCriteria key, HashSet<string> value);

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        void AddOrUpdate(ILookupCriteria key, string value);

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new string to add</param>
        void AddOrUpdateToCluster(ILookupCriteria key, string value);

        /// <summary>
        /// Gets a single value out by random
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        T GetSingleByRandom<T>(ILookupCriteria key);

        /// <summary>
        /// Gets the entire value cluster out
        /// </summary>
        /// <param name="key">the key of the values to get</param>
        /// <returns>the values</returns>
        IEnumerable<T> Get<T>(ILookupCriteria key);
    }
}
