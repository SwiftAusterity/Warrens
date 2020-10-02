using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    [Serializable]
    public class RelatedWord : IRelatedWord
    {
        [JsonProperty("Word")]
        private DictataKey _Word { get; set; }

        /// <summary>
        /// When the from word is ally this
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Word", Description = "When the From word is this or a synonym of this (only native synonyms) this rule applies.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata Word
        {
            get
            {
                if (_Word == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILexeme>(_Word.LexemeKey, ConfigDataType.Dictionary)?.GetForm(_Word.FormId);
            }
            set
            {
                if (value == null)
                {
                    _Word = null;
                    return;
                }

                _Word = new DictataKey(new ConfigDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }

        /// <summary>
        /// Personage of the word
        /// </summary>
        [Display(Name = "Relation Type", Description = "Synonym, antonym or other.")]
        [UIHint("EnumDropDownList")]
        [Required] 
        public WordRelationalType RelationType { get; set; }

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

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        [Display(Name = "Semantic Tags", Description = "Tags that describe the purpose/meaning of the word. (like Food or Positional)")]
        [UIHint("TagContainer")]
        public HashSet<string> Semantics { get; set; }

        public RelatedWord()
        {
            Semantics = new HashSet<string>();
        }
    }
}
