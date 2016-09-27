using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    public interface IConstants : IFileStored, IData
    {
        /// <summary>
        /// All string values
        /// </summary>
        Dictionary<string, string[]> Strings { get; set; }

        /// <summary>
        /// All numerical values
        /// </summary>
        Dictionary<string, double[]> Values { get; set; }

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new strings to add</param>
        void AddOrUpdateString(string key, string[] value);

        /// <summary>
        /// Adds or updates an entire numerical cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        void AddOrUpdateNumerical(string key, double[] value);

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        void AddOrUpdateString(string key, string value);

        /// <summary>
        /// Adds or updates an entire numerical cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        void AddOrUpdateNumerical(string key, double value);

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new string to add</param>
        void AddOrUpdateToStringCluster(string key, string value);

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new value to add</param>
        void AddOrUpdateToNumericalCluster(string key, double value);

        /// <summary>
        /// Gets a single string value out (will use first)
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        string GetSingleString(string key);

        /// <summary>
        /// Gets a single numerical value out (will use first)
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        double GetSingleNumerical(string key);
    }
}
