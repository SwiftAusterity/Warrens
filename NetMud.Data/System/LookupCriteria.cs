using NetMud.DataAccess;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace NetMud.Data.System
{
    /// <summary>
    /// Collection of lookup parameters for finding string constants
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(LookupCriteriaTypeConverter))]
    public class LookupCriteria : ILookupCriteria
    {
        /// <summary>
        /// The type of criteria to look for
        /// </summary>
        [JsonProperty("Criteria")]
        public Dictionary<CriteriaType, string> Criterion { get; set; }

        /// <summary>
        /// Instansiate with empty list of criteria
        /// </summary>
        public LookupCriteria()
        {
            Criterion = new Dictionary<CriteriaType, string>();
        }

        /// <summary>
        /// Instansiate with existing criteria list
        /// </summary>
        /// <param name="criteria">list of lookup criteria</param>
        [JsonConstructor]
        public LookupCriteria(Dictionary<CriteriaType, string> criteria)
        {
            Criterion = criteria;
        }

        /// <summary>
        /// -99 = null input
        ///-1 = Matches none
        /// 0 = Matches some
        /// 1 = matches all
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILookupCriteria other)
        {
            if (other != null)
            {
                try
                {
                    if (other.Criterion == Criterion)
                        return 1;

                    if (other.Criterion.Any(crit => Criterion.ContainsKey(crit.Key) && Criterion[crit.Key].Equals(crit.Value)))
                        return 0;

                    return -1;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILookupCriteria other)
        {
            if (other != default(ILookupCriteria))
            {
                try
                {
                    return other.Criterion.All(crit => Criterion.ContainsKey(crit.Key) && Criterion[crit.Key].Equals(crit.Value));
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Overriding this to make the json converter work right
        /// </summary>
        /// <returns>the json string</returns>
        public override string ToString()
        {
            var serializer = SerializationUtility.GetSerializer();

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, Criterion);

            return sb.ToString();
        }
    }
}
