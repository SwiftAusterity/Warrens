using NetMud.Data.Architectural;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Sort of a partial class of Lexica so it can get stored more easily and work for the processor
    /// </summary>
    [Serializable]
    public class DictataPhrase : ConfigData, IDictataPhrase, IComparable<IDictataPhrase>, IEquatable<IDictataPhrase>, IEqualityComparer<IDictataPhrase>
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.ReviewOnly;

        /// <summary>
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string UniqueKey => string.Format("{0}_{1}", Language.Name, Name);

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Dictionary;

        /// <summary>
        /// The unique name of this configuration data
        /// </summary>
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 1)]
        [Display(Name = "Full Phrase", Description = "The actual phrase.")]
        [DataType(DataType.Text)]
        [ScriptIgnore]
        [JsonIgnore]
        public override string Name
        {
            get
            {
                if(Words == null || Words.Count() == 0)
                {
                    return string.Empty;
                }

                return string.Join(" ", Words.Select(word => word.Name));
            }
            set
            {
                //nothing
            }
        }

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        [Display(Name = "Tense", Description = "Chronological tense (past, present, future)")]
        [UIHint("EnumDropDownList")]
        [Required]
        public LexicalTense Tense { get; set; }

        /// <summary>
        /// Does this indicate some sort of relational positioning
        /// </summary>
        [Display(Name = "Positional", Description = "Does this indicate some sort of relational positioning")]
        [UIHint("EnumDropDownList")]
        [Required]
        public LexicalPosition Positional { get; set; }

        /// <summary>
        /// Personage of the word
        /// </summary>
        [Display(Name = "Perspective", Description = "Narrative personage for the word.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public NarrativePerspective Perspective { get; set; }

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        [Display(Name = "Semantic Tags", Description = "Tags that describe the purpose/meaning of the word. (like Food or Positional)")]
        [UIHint("TagContainer")]
        public HashSet<string> Semantics { get; set; }

        /// <summary>
        /// Is this a feminine or masculine word
        /// </summary>
        [Display(Name = "Feminine Form", Description = "Is this a feminine or masculine word? (only applies to gendered languages)")]
        [UIHint("Boolean")]
        public bool Feminine { get; set; }

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Severity", Description = "Strength rating of word in relation to synonyms.")]
        [DataType(DataType.Text)]
        public int Severity { get; set; }

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Elegance", Description = "Crudeness rating of word in relation to synonyms.")]
        [DataType(DataType.Text)]
        public int Elegance { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Quality", Description = "Finesse synonym rating; quality of execution of form or function.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        [JsonProperty("Language")]
        private ConfigDataCacheKey _language { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Language", Description = "The language this is in.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        [Required]
        public ILanguage Language
        {
            get
            {
                if (_language == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILanguage>(_language);
            }
            set
            {
                if (value == null)
                {
                    _language = null;
                    return;
                }

                _language = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("Words")]
        private HashSet<ConfigDataCacheKey> _words { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Words", Description = "The words in this phrase.")]
        [UIHint("CollectionPhraseWordsList")]
        [DictataCollectionDataBinder]
        public HashSet<IDictata> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = new HashSet<ConfigDataCacheKey>();
                }

                return new HashSet<IDictata>(_words.Select(k => ConfigDataCache.Get<IDictata>(k)));
            }
            set
            {
                if (value == null)
                {
                    _words = new HashSet<ConfigDataCacheKey>();
                    return;
                }

                _words = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonProperty("Synonyms")]
        private HashSet<ConfigDataCacheKey> _synonyms { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Synonyms", Description = "The synonyms (similar) of this word/phrase.")]
        [UIHint("CollectionSynonymList")]
        [DictataCollectionDataBinder]
        public HashSet<IDictata> Synonyms
        {
            get
            {
                if (_synonyms == null)
                {
                    _synonyms = new HashSet<ConfigDataCacheKey>();
                }

                return new HashSet<IDictata>(_synonyms.Select(k => ConfigDataCache.Get<IDictata>(k)));
            }
            set
            {
                if (value == null)
                {
                    _synonyms = new HashSet<ConfigDataCacheKey>();
                    return;
                }

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
        [Display(Name = "Antonyms", Description = "The antonyms (opposite) of this word/phrase.")]
        [UIHint("CollectionDictataList")]
        [DictataCollectionDataBinder]
        public HashSet<IDictata> Antonyms
        {
            get
            {
                if (_antonyms == null)
                {
                    _antonyms = new HashSet<ConfigDataCacheKey>();
                }

                return new HashSet<IDictata>(_antonyms.Select(k => ConfigDataCache.Get<IDictata>(k)));
            }
            set
            {
                if (value == null)
                {
                    _antonyms = new HashSet<ConfigDataCacheKey>();
                    return;
                }

                _antonyms = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonProperty("PhraseSynonyms")]
        private HashSet<ConfigDataCacheKey> _phraseSynonyms { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Phrase Synonyms", Description = "The synonyms (similar) of this phrase.")]
        [UIHint("CollectionPhraseSynonymList")]
        [DictataCollectionDataBinder]
        public HashSet<IDictataPhrase> PhraseSynonyms
        {
            get
            {
                if (_phraseSynonyms == null)
                {
                    _phraseSynonyms = new HashSet<ConfigDataCacheKey>();
                }

                return new HashSet<IDictataPhrase>(_phraseSynonyms.Select(k => ConfigDataCache.Get<IDictataPhrase>(k)));
            }
            set
            {
                if (value == null)
                {
                    _phraseSynonyms = new HashSet<ConfigDataCacheKey>();
                    return;
                }

                _phraseSynonyms = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonProperty("PhraseAntonyms")]
        private HashSet<ConfigDataCacheKey> _phraseAntonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Phrase Antonyms", Description = "The antonyms (opposite) of this phrase.")]
        [UIHint("CollectionPhraseAntonymList")]
        [DictataCollectionDataBinder]
        public HashSet<IDictataPhrase> PhraseAntonyms
        {
            get
            {
                if (_phraseAntonyms == null)
                {
                    _phraseAntonyms = new HashSet<ConfigDataCacheKey>();
                }

                return new HashSet<IDictataPhrase>(_phraseAntonyms.Select(k => ConfigDataCache.Get<IDictataPhrase>(k)));
            }
            set
            {
                if (value == null)
                {
                    _phraseAntonyms = new HashSet<ConfigDataCacheKey>();
                    return;
                }

                _phraseAntonyms = new HashSet<ConfigDataCacheKey>(value.Select(k => new ConfigDataCacheKey(k)));
            }
        }

        [JsonConstructor]
        public DictataPhrase()
        {
            Words = new HashSet<IDictata>();
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            PhraseAntonyms = new HashSet<IDictataPhrase>();
            PhraseSynonyms = new HashSet<IDictataPhrase>();
            Semantics = new HashSet<string>();
            Name = string.Empty;

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            if (globalConfig?.BaseLanguage == null)
            {
                Language = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
            }
            else
            {
                Language = globalConfig.BaseLanguage;
            }
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

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
                return CompareTo(other as IDictataPhrase);
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
        public int CompareTo(IDictataPhrase other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.Words.All(wordType => Words.Contains(wordType)))
                    {
                        return 1;
                    }

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
        public bool Equals(IDictataPhrase other)
        {
            if (other != default(IDictataPhrase))
            {
                try
                {
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.Words.All(wordType => Words.Contains(wordType));
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
        public bool Equals(IDictataPhrase x, IDictataPhrase y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(IDictataPhrase obj)
        {
            return obj.GetType().GetHashCode() + obj.Language.Name.GetHashCode() + obj.Name.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Language.Name.GetHashCode() + Name.GetHashCode();
        }

        public override object Clone()
        {
            return new DictataPhrase
            {
                Antonyms = Antonyms,
                PhraseAntonyms = PhraseAntonyms,
                Elegance = Elegance,
                Language = Language,
                Severity = Severity,
                Synonyms = Synonyms,
                PhraseSynonyms = PhraseSynonyms,
                Tense = Tense,
                Words = Words,
                Quality = Quality
            };
        }
        #endregion

        #region Data persistence functions
        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool Remove(IAccount remover, StaffRank rank)
        {
            var removalState = base.Remove(remover, rank);

            if(removalState)
            {
                var synonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.PhraseSynonyms.Any(syn => syn.Equals(this)));
                var antonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.PhraseAntonyms.Any(ant => ant.Equals(this)));
                var synonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.PhraseSynonyms.Any(syn => syn.Equals(this)));
                var antonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.PhraseAntonyms.Any(ant => ant.Equals(this)));

                foreach (var word in synonyms)
                {
                    var syns = new HashSet<IDictataPhrase>(word.PhraseSynonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    word.PhraseSynonyms = syns;
                    word.Save(remover, rank);
                }

                foreach (var word in antonyms)
                {
                    var ants = new HashSet<IDictataPhrase>(word.PhraseAntonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    word.PhraseAntonyms = ants;
                    word.Save(remover, rank);
                }

                foreach (var phrase in synonymPhrases)
                {
                    var syns = new HashSet<IDictataPhrase>(phrase.PhraseSynonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    phrase.PhraseSynonyms = syns;
                    phrase.Save(remover, rank);
                }

                foreach (var phrase in antonymPhrases)
                {
                    var ants = new HashSet<IDictataPhrase>(phrase.PhraseAntonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    phrase.PhraseAntonyms = ants;
                    phrase.Save(remover, rank);
                }
            }

            return removalState;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save(IAccount editor, StaffRank rank)
        {
            var removalState = base.Remove(editor, rank);

            if (removalState)
            {
                return base.Save(editor, rank);
            }

            return false;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemSave()
        {
            var removalState = base.SystemRemove();

            if (removalState)
            {
                return base.SystemSave();
            }

            return false;
        }

        /// <summary>
        /// Remove this object from the db permenantly
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemRemove()
        {
            var removalState = base.SystemRemove();

            if (removalState)
            {
                var synonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.PhraseSynonyms.Any(syn => syn.Equals(this)));
                var antonyms = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.PhraseAntonyms.Any(ant => ant.Equals(this)));
                var synonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.PhraseSynonyms.Any(syn => syn.Equals(this)));
                var antonymPhrases = ConfigDataCache.GetAll<IDictataPhrase>().Where(dict => dict.PhraseAntonyms.Any(ant => ant.Equals(this)));

                foreach (var word in synonyms)
                {
                    var syns = new HashSet<IDictataPhrase>(word.PhraseSynonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    word.PhraseSynonyms = syns;
                    word.SystemSave();
                }

                foreach (var word in antonyms)
                {
                    var ants = new HashSet<IDictataPhrase>(word.PhraseAntonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    word.PhraseAntonyms = ants;
                    word.SystemSave();
                }

                foreach (var phrase in synonymPhrases)
                {
                    var syns = new HashSet<IDictataPhrase>(phrase.PhraseSynonyms);
                    syns.RemoveWhere(syn => syn.Equals(this));
                    phrase.PhraseSynonyms = syns;
                    phrase.SystemSave();
                }

                foreach (var phrase in antonymPhrases)
                {
                    var ants = new HashSet<IDictataPhrase>(phrase.PhraseAntonyms);
                    ants.RemoveWhere(syn => syn.Equals(this));
                    phrase.PhraseAntonyms = ants;
                    phrase.SystemSave();
                }
            }

            return removalState;
        }
        #endregion
    }
}
