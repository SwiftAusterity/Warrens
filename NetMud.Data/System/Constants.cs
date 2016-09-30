using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.System
{
    /// <summary>
    /// For handling world constant values for reference
    /// </summary>
    [Serializable]
    public class Constants : BackingDataPartial, IConstants
    {
        /// <summary>
        /// All string values
        /// </summary>
        public Dictionary<ILookupCriteria, string[]> Values { get; set; }

        /// <summary>
        /// Empty constructor for serialization
        /// </summary>
        public Constants()
        {
            Values = new Dictionary<ILookupCriteria, string[]>();
        }

        /// <summary>
        /// Adds or updates an entire string cluster
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new strings to add</param>
        public void AddOrUpdate(ILookupCriteria key, string[] value)
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
        public void AddOrUpdate(ILookupCriteria key, string value)
        {
            AddOrUpdate(key, new string[] { value });
        }

        /// <summary>
        /// Adds a single value to an existing cluster (or adds the cluster)
        /// </summary>
        /// <param name="key">the value to affect</param>
        /// <param name="value">the new string to add</param>
        public void AddOrUpdateToCluster(ILookupCriteria key, string value)
        {
            if (Values.ContainsKey(key))
            {
                var valueArray = Values[key].ToList();

                valueArray.Add(value);

                AddOrUpdate(key, valueArray.ToArray());
            }
            else
                AddOrUpdate(key, value);
        }

        /// <summary>
        /// Gets a single value out by random
        /// </summary>
        /// <param name="key">the key of the value to get</param>
        /// <returns>the value</returns>
        public T GetSingleByRandom<T>(ILookupCriteria key)
        {
            var cluster = Values[key];
            T value = default(T);

            if (cluster != null && cluster.Count() > 0)
            {
                if (cluster.Count() <= 1)
                    value = DataUtility.TryConvert<T>(cluster.FirstOrDefault());
                else
                    value = DataUtility.TryConvert<T>(cluster.OrderBy(v => Guid.NewGuid()).FirstOrDefault());
            }

            return value; 
        }

        /// <summary>
        /// Gets the entire value cluster out
        /// </summary>
        /// <param name="key">the key of the values to get</param>
        /// <returns>the values</returns>
        public IEnumerable<T> Get<T>(ILookupCriteria key)
        {
            var cluster = Values[key];

            if (cluster == null)
                return new T[] { default(T) };

            return cluster.Select(c => DataUtility.TryConvert<T>(c));
        }
    }
}
