using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Relational word pair rule for sentence construction
    /// </summary>
    public class WordPairRule : IWordPairRule
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
        private DictataKey _specificWord { get; set; }

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

                return ConfigDataCache.Get<ILexeme>(_specificWord.LexemeKey)?.GetForm(_specificWord.FormId);
            }
            set
            {
                if (value == null)
                {
                    _specificWord = null;
                    return;
                }

                _specificWord = new DictataKey(new ConfigDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }

        [JsonProperty("SpecificAddition")]
        private DictataKey _specificAddition { get; set; }

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

                return ConfigDataCache.Get<ILexeme>(_specificAddition.LexemeKey)?.GetForm(_specificAddition.FormId);
            }
            set
            {
                if (value == null)
                {
                    _specificAddition = null;
                    return;
                }

                _specificAddition = new DictataKey(new ConfigDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }

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
        /// Only applies when the context has a position
        /// </summary>
        [Display(Name = "When Positional", Description = "Only when the word indicates relative position.")]
        [UIHint("Boolean")]
        public bool WhenPositional { get; set; }

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
        [Display(Name = "From Semantic", Description = "When the origin word has this semantic tag. Can be | delimited. (All are required to match.)")]
        [DataType(DataType.Text)]
        public string FromSemantics { get; set; }

        /// <summary>
        /// Only when the word ends with
        /// </summary>
        [Display(Name = "From Ends With", Description = "Only when the origin word ends with this string. Can be | delimited.")]
        [DataType(DataType.Text)]
        public string FromEndsWith { get; set; }

        /// <summary>
        /// Only when the word begins with
        /// </summary>
        [Display(Name = "From Begins With", Description = "Only when the origin word begins with this string. Can be | delimited.")]
        [DataType(DataType.Text)]
        public string FromBeginsWith { get; set; }

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
        [Display(Name = "To Semantic", Description = "When the modifying word has this semantic tag. Can be | delimited. (all are required to match)")]
        [DataType(DataType.Text)]
        public string ToSemantics { get; set; }

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

        public WordPairRule()
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
            FromBeginsWith = string.Empty;
            FromEndsWith = string.Empty;
        }

        /// <summary>
        /// Rate this rule on how specific it is so we can run the more specific rules first
        /// </summary>
        /// <returns>Specificity rating, higher = more specific</returns>
        public int RuleSpecificity()
        {
            return (string.IsNullOrWhiteSpace(ToSemantics) ? 0 : ToSemantics.Count(ch => ch == '|') * 3 + 1) +
                    (string.IsNullOrWhiteSpace(FromSemantics) ? 0 : FromSemantics.Count(ch => ch == '|') * 3 + 1) +
                    (string.IsNullOrWhiteSpace(FromEndsWith) ? 0 : FromEndsWith.Count(ch => ch == '|') + 3) +
                    (string.IsNullOrWhiteSpace(FromBeginsWith) ? 0 : FromBeginsWith.Count(ch => ch == '|') + 3) +
                    (SpecificWord == null ? 0 : 99) +
                    (Tense == LexicalTense.None ? 0 : 2) +
                    (Perspective == NarrativePerspective.None ? 0 : 2) +
                    (ToType == LexicalType.None ? 0 : 3) +
                    (ToRole == GrammaticalType.None ? 0 : 3) +
                    (FromType == LexicalType.None ? 0 : 3) +
                    (FromRole == GrammaticalType.None ? 0 : 3) +
                    (WhenPlural ? 5 : 0) +
                    (WhenPositional ? 10 : 0) +
                    (WhenPossessive ? 5 : 0);
        }

        /// <summary>
        /// Does this lexica match the rule
        /// </summary>
        /// <param name="word">The lex</param>
        /// <returns>if it matches</returns>
        public bool Matches(ILexica lex, ILexica pairedWord)
        {
            var fromBegins = FromBeginsWith.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var fromEnds = FromEndsWith.Split('|', StringSplitOptions.RemoveEmptyEntries);

            return (ToRole == GrammaticalType.None || ToRole == pairedWord.Role)
                    && (ToType == LexicalType.None || ToType == pairedWord.Type)
                    && (fromBegins.Count() == 0 || fromBegins.Any(bw => lex.Phrase.StartsWith(bw)))
                    && (fromEnds.Count() == 0 || fromEnds.Any(bw => lex.Phrase.EndsWith(bw)))
                    && (Tense == LexicalTense.None || lex.Context.Tense == Tense)
                    && (Perspective == NarrativePerspective.None || lex.Context.Perspective == Perspective)
                    && (!WhenPlural || lex.Context.Plural)
                    && (!WhenPossessive || lex.Context.Possessive)
                    && (!WhenPositional || pairedWord.Context.Position != LexicalPosition.None)
                    && (SpecificWord == null || SpecificWord.Equals(lex.GetDictata()))
                    && (SpecificWord != null || ((FromRole == GrammaticalType.None || FromRole == lex.Role) && (FromType == LexicalType.None || FromType == lex.Type)));
        }
    }
}
