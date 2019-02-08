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

        /// <summary>
        /// The UI language for output purposes
        /// </summary>
        [JsonProperty("SpecificWord")]
        private ConfigDataCacheKey _specificWord { get; set; }

        /// <summary>
        /// The UI language for output purposes
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
        /// Can be made into a list
        /// </summary>
        [Display(Name = "Listable", Description = "Can be made into a list.")]
        [UIHint("Boolean")]
        public bool Listable { get; set; }

        /// <summary>
        /// Place the "to" word before the From word, false means after
        /// </summary>
        [Display(Name = "Precedes", Description = "Place the to word before the From word, false means after.")]
        [UIHint("Boolean")]
        public bool Precedes { get; set; }

        /// <summary>
        /// The presence of these criteria changes the sentence type
        /// </summary>
        [Display(Name = "Alters Sentence Type", Description = "The presence of these criteria changes the sentence type.")]
        [UIHint("EnumDropDownList")]
        public SentenceType AltersSentence { get; set; }
    }
}
