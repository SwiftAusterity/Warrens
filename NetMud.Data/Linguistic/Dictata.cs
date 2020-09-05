using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
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
    public class Dictata : IDictata, IComparable<IDictata>, IEquatable<IDictata>, IEqualityComparer<IDictata>
    {
        /// <summary>
        /// The unique key language_name_id
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public string UniqueKey => string.Format("{0}_{1}_{2}", Language.Name, Name, FormGroup);

        /// <summary>
        /// The text of the word
        /// </summary>
        [Display(Name = "Name", Description = "The lexeme this form belongs to.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// The grouping value for synonym tracking
        /// </summary>
        public short FormGroup { get; set; }

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


        /// <summary>
        /// Human readable definition
        /// </summary>
        [Display(Name = "Definition", Description = "What this means.")]
        [UIHint("Markdown")]
        public MarkdownString Definition { get; set; }

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
        /// The number of times this specific wordform has been rated
        /// </summary>
        [Display(Name = "Rating Count", Description = "The number of times this specific wordform has been rated.")]
        [DataType(DataType.Text)]
        public int TimesRated { get; set; }

        /// <summary>
        /// Usage context
        /// </summary>
        [Display(Name = "Usage", Description = "Usage context for the word.")]
        [UIHint("EnumDropDownList")]
        public SemanticContext Context { get; set; }

        /// <summary>
        /// Synonym status for offensive
        /// </summary>
        [Display(Name = "Vulgar", Description = "Is this considered vulgar?")]
        [UIHint("Boolean")]
        public bool Vulgar { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Quality", Description = "Finesse synonym rating; quality of execution of form or function.")]
        [DataType(DataType.Text)]
        public int Quality { get; set; }

        [JsonProperty("Synonyms")]
        private HashSet<DictataKey> _synonyms { get; set; }

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
                    _synonyms = new HashSet<DictataKey>();
                }

                return new HashSet<IDictata>(_synonyms.Select(k => LuceneDataCache.Get<ILexeme>(k.LexemeKey)?.GetForm(k.FormId)));
            }
            set
            {
                if (value == null)
                {
                    _synonyms = new HashSet<DictataKey>();
                    return;
                }

                _synonyms = new HashSet<DictataKey>(
                    value.Where(k => k != null).Select(k => new DictataKey(new LuceneDataCacheKey(k.GetLexeme()).BirthMark, k.FormGroup)));
            }
        }

        [JsonProperty("Antonyms")]
        private HashSet<DictataKey> _antonyms { get; set; }

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
                    _antonyms = new HashSet<DictataKey>();
                }

                return new HashSet<IDictata>(_antonyms.Select(k => LuceneDataCache.Get<ILexeme>(k.LexemeKey)?.GetForm(k.FormId)));
            }
            set
            {
                if (value == null)
                {
                    _antonyms = new HashSet<DictataKey>();
                    return;
                }

                _antonyms = new HashSet<DictataKey>(
                    value.Where(k => k != null).Select(k => new DictataKey(new LuceneDataCacheKey(k.GetLexeme()).BirthMark, k.FormGroup)));
            }
        }


        [JsonConstructor]
        public Dictata()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            if (globalConfig?.BaseLanguage == null)
            {
                Language = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
            }
            else
            {
                Language = globalConfig.BaseLanguage;
            }

            Name = "";
            WordType = LexicalType.None;
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            Semantics = new HashSet<string>();
        }

        public Dictata(string name, short formGrouping)
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            if (globalConfig?.BaseLanguage == null)
            {
                Language = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
            }
            else
            {
                Language = globalConfig.BaseLanguage;
            }

            Name = name.ToLower();
            FormGroup = formGrouping;
            WordType = LexicalType.None;
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            Semantics = new HashSet<string>();
        }

        /// <summary>
        /// Make a dictata from a lexica
        /// </summary>
        /// <param name="lexica">the incoming lexica phrase</param>
        public Dictata(ILexica lexica)
        {
            Name = "";
            Antonyms = new HashSet<IDictata>();
            Synonyms = new HashSet<IDictata>();
            Semantics = new HashSet<string>();

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            if (lexica.Context?.Language == null)
            {
                if (globalConfig?.BaseLanguage == null)
                {
                    Language = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();
                }
                else
                {
                    Language = globalConfig.BaseLanguage;
                }
            }
            else
            {
                Language = lexica.Context.Language;
            }

            ILexeme maybeLex = LuceneDataCache.Get<ILexeme>(
                new LuceneDataCacheKey(typeof(ILexeme), string.Format("{0}_{1}", Language.Name, lexica.Phrase)));

            if (maybeLex == null)
            {
                Name = lexica.Phrase;
                WordType = lexica.Type;
            }
            else if (maybeLex.WordForms.Any(form => form.WordType == lexica.Type))
            {
                IDictata wordForm = maybeLex.WordForms.FirstOrDefault(form => form.WordType == lexica.Type);

                Name = lexica.Phrase;
                WordType = lexica.Type;
                Synonyms = wordForm.Synonyms;
                Antonyms = wordForm.Antonyms;
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
        public ILexica GetLexica(GrammaticalType role, LexicalContext context)
        {
            return new Lexica(WordType, role, Name, context);
        }

        /// <summary>
        /// creates a related dictata and lexeme with a new word
        /// </summary>
        /// <param name="synonym"></param>
        /// <returns></returns>
        public ILexeme MakeRelatedWord(ILanguage language, string word, bool synonym, IDictata existingWord = null)
        {
            ILexeme possibleLex = LuceneDataCache.Get<ILexeme>(new LuceneDataCacheKey(typeof(ILexeme), string.Format("{0}_{1}", language.Name, word)));

            if (possibleLex == null)
            {
                possibleLex = new Lexeme()
                {
                    Name = word,
                    IsSynMapped = false,
                    Language = language
                };
            }

            IDictata newDict = existingWord;
            if (newDict == null)
            {
                newDict = new Dictata()
                {
                    Name = word,
                    Language = language,
                    Severity = Severity,
                    Quality = Quality,
                    Elegance = Elegance,
                    Tense = Tense,
                    WordType = WordType,
                    Feminine = Feminine,
                    Possessive = Possessive,
                    Plural = Plural,
                    Determinant = Determinant,
                    Positional = Positional,
                    Perspective = Perspective,
                    Semantics = Semantics
                };
            }

            HashSet<IDictata> synonyms = Synonyms;
            synonyms.Add(this);

            if (synonym)
            {
                newDict.Synonyms = synonyms;
                newDict.Antonyms = Antonyms;

                HashSet<IDictata> mySynonyms = Synonyms;
                mySynonyms.Add(newDict);

                Synonyms = mySynonyms;
            }
            else
            {
                newDict.Synonyms = Antonyms;
                newDict.Antonyms = synonyms;

                HashSet<IDictata> antonyms = Antonyms;
                antonyms.Add(newDict);

                Antonyms = antonyms;
            }

            possibleLex.AddNewForm(newDict);
            possibleLex.PersistToCache();
            possibleLex.SystemSave();

            possibleLex.MapSynNet();

            var myLex = GetLexeme();
            myLex.SystemSave();
            myLex.PersistToCache();

            return possibleLex;
        }

        /// <summary>
        /// Get the lexeme for this word
        /// </summary>
        /// <returns>the lexeme</returns>
        public ILexeme GetLexeme()
        {
            ILexeme lex = LuceneDataCache.Get<ILexeme>(string.Format("{0}_{1}", Language.Name, Name));

            if (lex != null)
            {
                if (!lex.WordForms.Any(form => form == this))
                {
                    lex.AddNewForm(this);
                    lex.SystemSave();
                    lex.PersistToCache();
                }
            }
            else
            {
                lex = new Lexeme()
                {
                    Name = Name,
                    Language = Language
                };

                lex.SystemSave();
                lex.PersistToCache();

                lex.AddNewForm(this);
            }

            return lex;
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

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.WordType == WordType
                        && Semantics.Count() == other.Semantics.Count() && Semantics.All(semantic => other.Semantics.Contains(semantic)))
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
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) && other.WordType == WordType
                        && Semantics.Count() == other.Semantics.Count() && Semantics.All(semantic => other.Semantics.Contains(semantic));
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
            return obj.GetType().GetHashCode() + obj.Name.GetHashCode() + obj.WordType.GetHashCode() + obj.Semantics.Count();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Name.GetHashCode() + WordType.GetHashCode() + Semantics.Count();
        }

        public IDictata Clone()
        {
            return new Dictata
            {
                Elegance = Elegance,
                Severity = Severity,
                Antonyms = Antonyms,
                Synonyms = Synonyms,
                Tense = Tense,
                WordType = WordType,
                Quality = Quality
            };
        }
        #endregion
    }
}
