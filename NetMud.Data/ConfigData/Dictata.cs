using NetMud.Communication.Messaging;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// Sort of a partial class of Lexica so it can get stored more easily and work for the processor
    /// </summary>
    [Serializable]
    public class Dictata : ConfigData, IDictata, IComparable<IDictata>, IEquatable<IDictata>, IEqualityComparer<IDictata>
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.ReviewOnly;

        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string UniqueKey => string.Format("{0}_{1}", WordType.ToString(), Name);

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Dictionary;

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        public LexicalType WordType { get; set; }

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        public LexicalTense Tense { get; set; }

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        public int Severity { get; set; }

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        public int Elegance { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        public int Quality { get; set; }

        [JsonProperty("Language")]
        private ConfigDataCacheKey _language { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ILanguage Language
        {
            get
            {
                if (_language == null)
                    return null;

                return ConfigDataCache.Get<ILanguage>(_language);
            }
            set
            {
                if (value == null)
                    _language = null;

                _language = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("Synonyms")]
        private HashSet<ConfigDataCacheKey> _synonyms { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IDictata> Synonyms
        {
            get
            {
                if (_synonyms == null)
                    _synonyms = new HashSet<ConfigDataCacheKey>();

                return _synonyms.Select(k => ConfigDataCache.Get<IDictata>(k));
            }
            set
            {
                if (value == null)
                    return;

                _synonyms = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonProperty("Antonyms")]
        private HashSet<ConfigDataCacheKey> _antonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IDictata> Antonyms
        {
            get
            {
                if (_antonyms == null)
                    _antonyms = new HashSet<ConfigDataCacheKey>();

                return _antonyms.Select(k => ConfigDataCache.Get<IDictata>(k));
            }
            set
            {
                if (value == null)
                    return;

                _antonyms = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonConstructor]
        public Dictata()
        {
            Antonyms = Enumerable.Empty<IDictata>();
            Synonyms = Enumerable.Empty<IDictata>();
        }


        /// <summary>
        /// Make a dictata from a lexica
        /// </summary>
        /// <param name="lexica">the incoming lexica phrase</param>
        public Dictata(ILexica lexica)
        {
            Antonyms = Enumerable.Empty<IDictata>();
            Synonyms = Enumerable.Empty<IDictata>();

            Name = lexica.Phrase;
            WordType = lexica.Type;
        }

        /// <summary>
        /// Get this in lexica form
        /// </summary>
        /// <returns>A Lexica with the same values</returns>
        public ILexica GetLexica()
        {
            return new Lexica(WordType, GrammaticalType.Subject, Name);
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("WordType", WordType.ToString());

            return returnList;
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            try
            {
                return CompareTo(other as IDictata);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return -99;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(IDictata other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.WordType == WordType)
                        return 1;

                    return 0;
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
        public bool Equals(IDictata other)
        {
            if (other != default(IDictata))
            {
                try
                {
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.WordType == WordType;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(IDictata x, IDictata y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(IDictata obj)
        {
            return obj.GetType().GetHashCode() + obj.WordType.GetHashCode() + obj.Name.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + WordType.GetHashCode() + Name.GetHashCode();
        }
        #endregion
    }
}
