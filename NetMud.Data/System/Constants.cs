using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.System
{
    /// <summary>
    /// Class for handling world constant values for reference
    /// </summary>
    [Serializable]
    public class Constants : BackingDataPartial, IConstants
    {
        /// <summary>
        /// All string values
        /// </summary>
        public Dictionary<string, string[]> Strings { get; set; }

        /// <summary>
        /// All numerical values
        /// </summary>
        public Dictionary<string, double[]> Values { get; set; }

        /// <summary>
        /// Empty constructor for serialization
        /// </summary>
        public Constants()
        {
            Strings = new Dictionary<string, string[]>();
            Values = new Dictionary<string, double[]>();
        }

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new strings to add</param>
        public void AddOrUpdateString(string key, string[] value)
        {
            if (Strings.ContainsKey(key))
                Strings.Remove(key);

            Strings.Add(key, value);
        }

        /// <summary>
        /// Adds or updates an entire numerical cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        public void AddOrUpdateNumerical(string key, double[] value)
        {
            if (Values.ContainsKey(key))
                Values.Remove(key);

            Values.Add(key, value);
        }

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        public void AddOrUpdateString(string key, string value)
        {
            AddOrUpdateString(key, new string[] { value });
        }

        /// <summary>
        /// Adds or updates an entire numerical cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new values to add</param>
        public void AddOrUpdateNumerical(string key, double value)
        {
            AddOrUpdateNumerical(key, new double[] { value });
        }

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new string to add</param>
        public void AddOrUpdateToStringCluster(string key, string value)
        {
            if (Strings.ContainsKey(key))
            {
                var valueArray = Strings[key].ToList();

                valueArray.Add(value);

                AddOrUpdateString(key, valueArray.ToArray());
            }
            else
                AddOrUpdateString(key, value);
        }

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new value to add</param>
        public void AddOrUpdateToNumericalCluster(string key, double value)
        {
            if (Values.ContainsKey(key))
            {
                var valueArray = Values[key].ToList();

                valueArray.Add(value);

                AddOrUpdateNumerical(key, valueArray.ToArray());
            }
            else
                AddOrUpdateNumerical(key, value);
        }

        /// <summary>
        /// Gets a single string value out (will use first)
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        public string GetSingleString(string key)
        {
            var cluster = Strings[key];

            if (cluster == null)
                return String.Empty;

            return cluster.First();
        }

        /// <summary>
        /// Gets a single numerical value out (will use first)
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        public double GetSingleNumerical(string key)
        {
            var cluster = Values[key];

            if (cluster == null)
                return -1;

            return cluster.First();
        }
    }
}
