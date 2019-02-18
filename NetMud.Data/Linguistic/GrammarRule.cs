using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Relational rule From sentence construction
    /// </summary>
    public class GrammarRule : IGrammarRule
    {
        /// <summary>
        /// Rule applies when sentence is in this tense
        /// </summary>
        [Display(Name = "Tense", Description = "Rule applies when sentence is in this tense.")]
        [UIHint("EnumDropDownList")]
        public LexicalTense Tense { get; set; }

        /// <summary>
        /// Rule applies when sentence is in this perspective
        /// </summary>
        [Display(Name = "Perspective", Description = "Rule applies when sentence is in this perspective.")]
        [UIHint("EnumDropDownList")]
        public NarrativePerspective Perspective { get; set; }

        [JsonProperty("SpecificWord")]
        private ConfigDataCacheKey _specificWord { get; set; }

        /// <summary>
        /// When the from word is specifically this
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Specific Word", Description = "When the From word is this or a synonym of this (only native synonyms) this rule applies.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata SpecificWord
        {
            get
            {
                if (_specificWord == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_specificWord);
            }
            set
            {
                if (value == null)
                {
                    _specificWord = null;
                    return;
                }

                _specificWord = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("SpecificAddition")]
        private ConfigDataCacheKey _specificAddition { get; set; }

        /// <summary>
        /// When the additional word (like the article) should be this explicitely
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Specific Addition", Description = "When the additional word (like the article being added) should be this explicitely.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata SpecificAddition
        {
            get
            {
                if (_specificAddition == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_specificAddition);
            }
            set
            {
                if (value == null)
                {
                    _specificAddition = null;
                    return;
                }

                _specificAddition = new ConfigDataCacheKey(value);
            }
        }

        /// <summary>
        /// Only when the word ends with
        /// </summary>
        [Display(Name = "When Ends With", Description = "Only when the word ends with this string.")]
        [DataType(DataType.Text)]
        public string WhenEndsWith { get; set; }

        /// <summary>
        /// Only when the word begins with
        /// </summary>
        [Display(Name = "When Begins With", Description = "Only when the word begins with this string.")]
        [DataType(DataType.Text)]
        public string WhenBeginsWith { get; set; }

        /// <summary>
        /// Only applies when the context is possessive
        /// </summary>
        [Display(Name = "When Possessive", Description = "Only when the word is possessive form.")]
        [UIHint("Boolean")]
        public bool WhenPossessive { get; set; }

        /// <summary>
        /// Only applies when the context is plural
        /// </summary>
        [Display(Name = "When Plural", Description = "Only when the word is pluralized.")]
        [UIHint("Boolean")]
        public bool WhenPlural { get; set; }

        /// <summary>
        /// Add this prefix
        /// </summary>
        [Display(Name = "Add Prefix", Description = " Add this prefix to the word.")]
        [DataType(DataType.Text)]
        public string AddPrefix { get; set; }

        /// <summary>
        /// Add this suffix
        /// </summary>
        [Display(Name = "Add Suffix", Description = " Add this suffix to the word.")]
        [DataType(DataType.Text)]
        public string AddSuffix { get; set; }

        /// <summary>
        /// Applies when this type of word is the primary one
        /// </summary>
        [Display(Name = "From Type", Description = "Applies when this type of word is the primary one.")]
        [UIHint("EnumDropDownList")]
        public LexicalType FromType { get; set; }

        /// <summary>
        /// This rule applies when the word is this role
        /// </summary>
        [Display(Name = "From Role", Description = "This rule applies when the word is this role.")]
        [UIHint("EnumDropDownList")]
        public GrammaticalType FromRole { get; set; }

        /// <summary>
        /// When the origin word has this semantic tag
        /// </summary>
        [Display(Name = "From Semantic", Description = "When the origin word has this semantic tag.")]
        [DataType(DataType.Text)]
        public string FromSemantics { get; set; }

        /// <summary>
        /// Applies when we're trying to figure out where to put this type of word
        /// </summary>
        [Display(Name = "To Type", Description = "Applies when we're trying to figure out where to put this type of word.")]
        [UIHint("EnumDropDownList")]
        public LexicalType ToType { get; set; }

        /// <summary>
        /// This rule applies when the adjunct word is this role
        /// </summary>
        [Display(Name = "To Role", Description = "This rule applies when the adjunct word is this role.")]
        [UIHint("EnumDropDownList")]
        public GrammaticalType ToRole { get; set; }

        /// <summary>
        /// When the modifying word has this semantic tag
        /// </summary>
        [Display(Name = "To Semantic", Description = "When the modifying word has this semantic tag.")]
        [DataType(DataType.Text)]
        public string ToSemantics { get; set; }

        /// <summary>
        /// Can be made into a list
        /// </summary>
        [Display(Name = "Listable", Description = "Can be made into a list.")]
        [UIHint("Boolean")]
        public bool Listable { get; set; }

        /// <summary>
        /// Where does the To word fit around the From word? (the from word == 0)
        /// </summary>
        [Display(Name = "Descriptive Order", Description = "Where does the To word fit around the From word? (the from word == 0)")]
        [DataType(DataType.Text)]
        public int ModificationOrder { get; set; }

        /// <summary>
        /// Does this word require an Article added (like nouns preceeding or verbs anteceding)
        /// </summary>
        [Display(Name = "Add Article", Description = "Does this word require an Article added? (like nouns preceeding or verbs anteceding)")]
        [UIHint("Boolean")]
        public bool NeedsArticle { get; set; }

        /// <summary>
        /// The presence of these criteria changes the sentence type
        /// </summary>
        [Display(Name = "Alters Sentence Type", Description = "The presence of these criteria changes the sentence type.")]
        [UIHint("EnumDropDownList")]
        public SentenceType AltersSentence { get; set; }

        public GrammarRule()
        {
            AltersSentence = SentenceType.None;
            NeedsArticle = false;
            WhenPlural = false;
            WhenPossessive = false;
            SpecificAddition = null;
            SpecificWord = null;
            Tense = LexicalTense.None;
            Perspective = NarrativePerspective.None;
            ToType = LexicalType.None;
            ToRole = GrammaticalType.None;
            FromType = LexicalType.None;
            FromRole = GrammaticalType.None;
            ToSemantics = string.Empty;
            FromSemantics = string.Empty;
            WhenBeginsWith = string.Empty;
            WhenEndsWith = string.Empty;
        }

        /// <summary>
        /// Rate this rule on how specific it is so we can run the more specific rules first
        /// </summary>
        /// <returns>Specificity rating, higher = more specific</returns>
        public int RuleSpecificity()
        {
            return (string.IsNullOrWhiteSpace(ToSemantics) ? 0 : 1) +
                    (string.IsNullOrWhiteSpace(FromSemantics) ? 0 : 1) +
                    (string.IsNullOrWhiteSpace(WhenEndsWith) ? 0 : 3) +
                    (string.IsNullOrWhiteSpace(WhenBeginsWith) ? 0 : 3) +
                    (SpecificWord == null ? 0 : 99) +
                    (Tense == LexicalTense.None ? 0 : 2) +
                    (Perspective == NarrativePerspective.None ? 0 : 2) +
                    (ToType == LexicalType.None ? 0 : 1) +
                    (ToRole == GrammaticalType.None ? 0 : 1) +
                    (FromType == LexicalType.None ? 0 : 3) +
                    (FromRole == GrammaticalType.None ? 0 : 3);
        }

        /// <summary>
        /// Does this lexica match the rule
        /// </summary>
        /// <param name="word">The lex</param>
        /// <returns>if it matches</returns>
        public bool Matches(ILexica lex, GrammaticalType toRole = GrammaticalType.None, LexicalType toType = LexicalType.None)
        {
            return (ToRole == GrammaticalType.None || ToRole == toRole)
                    && (ToType == LexicalType.None || ToType == toType)
                    && (string.IsNullOrWhiteSpace(WhenBeginsWith) || lex.Phrase.StartsWith(WhenBeginsWith))
                    && (string.IsNullOrWhiteSpace(WhenEndsWith) || lex.Phrase.EndsWith(WhenEndsWith))
                    && (Tense == LexicalTense.None || lex.Context.Tense == Tense)
                    && (Perspective == NarrativePerspective.None || lex.Context.Perspective == Perspective)
                    && (Tense == LexicalTense.None || lex.Context.Tense == Tense)
                    && (!WhenPlural || lex.Context.Plural)
                    && (!WhenPossessive || lex.Context.Possessive)
                    && ((SpecificWord != null && SpecificWord == lex.GetDictata())
                    || (FromRole == lex.Role && FromType == lex.Type));

        }
    }

    /// <summary>
    /// Rules that identify contractions
    /// </summary>
    public class ContractionRule : IContractionRule
    {
        [JsonProperty("First")]
        private ConfigDataCacheKey _first { get; set; }

        /// <summary>
        /// One of the words in the contraction (not an indicator of order)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "First", Description = "One of the words in the contraction (not an indicator of order).")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata First
        {
            get
            {
                if (_first == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_first);
            }
            set
            {
                if (value == null)
                {
                    _first = null;
                    return;
                }

                _first = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("Second")]
        private ConfigDataCacheKey _second { get; set; }

        /// <summary>
        /// One of the words in the contraction (not an indicator of order)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Second", Description = "One of the words in the contraction (not an indicator of order).")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata Second
        {
            get
            {
                if (_second == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_second);
            }
            set
            {
                if (value == null)
                {
                    _second = null;
                    return;
                }

                _second = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("Contraction")]
        private ConfigDataCacheKey _contraction { get; set; }

        /// <summary>
        /// The contraction this turns into
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Contraction", Description = "The contraction this turns into.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata Contraction
        {
            get
            {
                if (_contraction == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_contraction);
            }
            set
            {
                if (value == null)
                {
                    _contraction = null;
                    return;
                }

                _contraction = new ConfigDataCacheKey(value);
            }
        }
    }
}
