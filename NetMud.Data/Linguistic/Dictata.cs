using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
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
    public class Dictata : IDictata, IComparable<IDictata>, IEquatable<IDictata>, IEqualityComparer<IDictata>
    {
        /// <summary>
        /// The text of the word
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        [Display(Name = "Type", Description = "The type of word this is.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public LexicalType WordType { get; set; }

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
        /// Is this an determinant form or not (usually true)
        /// </summary>
        [Display(Name = "Determinant", Description = "Is this an determinant form or not? (usually true)")]
        [UIHint("Boolean")]
        public bool Determinant { get; set; }

        /// <summary>
        /// Is this a plural form
        /// </summary>
        [Display(Name = "Plural", Description = "Is this a plural form?")]
        [UIHint("Boolean")]
        public bool Plural { get; set; }

        /// <summary>
        /// Is this a possessive form
        /// </summary>
        [Display(Name = "Possessive", Description = "Is this a possessive form?")]
        [UIHint("Boolean")]
        public bool Possessive { get; set; }

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

        [JsonProperty("Synonyms")]
        private HashSet<Tuple<string, string>> _synonyms { get; set; }

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
                    _synonyms = new HashSet<Tuple<string, string>>();
                }

                return new HashSet<IDictata>(_synonyms.Select(k => ConfigDataCache.Get<IDictata>(k)));
            }
            set
            {
                if (value == null)
                {
                    _synonyms = new HashSet<Tuple<string, string>>();
                    return;
                }

                _synonyms = new HashSet<Tuple<string, string>>(value.Select(k => new ConfigDataCacheKey(k)));
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
        [DictataPhraseCollectionDataBinder]
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
        [DictataPhraseCollectionDataBinder]
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
        public Dictata()
        {
            WordType = LexicalType.None;
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            PhraseAntonyms = new HashSet<IDictataPhrase>();
            PhraseSynonyms = new HashSet<IDictataPhrase>();
            Semantics = new HashSet<string>();
        }

        public Dictata(string name)
        {
            Name = name;
            WordType = LexicalType.None;
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            PhraseAntonyms = new HashSet<IDictataPhrase>();
            PhraseSynonyms = new HashSet<IDictataPhrase>();
            Semantics = new HashSet<string>();
        }

        /// <summary>
        /// Make a dictata from a lexica
        /// </summary>
        /// <param name="lexica">the incoming lexica phrase</param>
        public Dictata(ILexica lexica)
        {
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            PhraseAntonyms = new HashSet<IDictataPhrase>();
            PhraseSynonyms = new HashSet<IDictataPhrase>();
            Semantics = new HashSet<string>();

            var maybeLex = ConfigDataCache.Get<ILexeme>(
                new ConfigDataCacheKey(typeof(ILexeme), string.Format("{0}_{1}", lexica.Context.Language.Name, lexica.Phrase), ConfigDataType.Dictionary));

            if (maybeLex == null)
            {
                Name = lexica.Phrase;
                WordType = lexica.Type;
            }
            else if (maybeLex.WordForms.Any(form => form.WordType == lexica.Type))
            {
                var wordForm = maybeLex.WordForms.FirstOrDefault(form => form.WordType == lexica.Type);

                WordType = lexica.Type;
                Synonyms = wordForm.Synonyms;
                Antonyms = wordForm.Antonyms;
                PhraseSynonyms = wordForm.PhraseSynonyms;
                PhraseAntonyms = wordForm.PhraseAntonyms;
                Determinant = wordForm.Determinant;
                Elegance = wordForm.Elegance;
                Feminine = wordForm.Feminine;
                Perspective = wordForm.Perspective;
                Plural = wordForm.Plural;
                Positional = wordForm.Positional;
                Possessive = wordForm.Possessive;
                Quality = wordForm.Quality;
                Semantics = wordForm.Semantics;
                Severity = wordForm.Severity;
                Tense = wordForm.Tense;
            }
        }

        /// <summary>
        /// Create a lexica from this
        /// </summary>
        /// <returns></returns>
        public ILexica GetLexica(GrammaticalType role, LexicalType type, LexicalContext context)
        {
            return new Lexica(type, role, Name, context);
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
                    {
                        return -1;
                    }

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.WordType == WordType)
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
            return obj.GetType().GetHashCode() + obj.Name.GetHashCode() + obj.WordType.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Name.GetHashCode() + WordType.GetHashCode();
        }

        public IDictata Clone()
        {
            return new Dictata
            {
                Elegance = Elegance,
                Severity = Severity,
                Antonyms = Antonyms,
                Synonyms = Synonyms,
                PhraseAntonyms = PhraseAntonyms,
                PhraseSynonyms = PhraseSynonyms,
                Tense = Tense,
                WordType = WordType,
                Quality = Quality
            };
        }
        #endregion
    }
}
