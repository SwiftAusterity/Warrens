using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Rules that identify contractions
    /// </summary>
    public class ContractionRule : IContractionRule
    {
        [JsonProperty("First")]
        private DictataKey _first { get; set; }

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

                return LuceneDataCache.Get<ILexeme>(_first.LexemeKey)?.GetForm(_first.FormId);
            }
            set
            {
                if (value == null)
                {
                    _first = null;
                    return;
                }

                _first = new DictataKey(new LuceneDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }

        [JsonProperty("Second")]
        private DictataKey _second { get; set; }

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

                return LuceneDataCache.Get<ILexeme>(_first.LexemeKey)?.GetForm(_second.FormId);
            }
            set
            {
                if (value == null)
                {
                    _second = null;
                    return;
                }

                _second = new DictataKey(new LuceneDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }

        [JsonProperty("Contraction")]
        private DictataKey _contraction { get; set; }

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

                return LuceneDataCache.Get<ILexeme>(_contraction.LexemeKey)?.GetForm(_contraction.FormId);
            }
            set
            {
                if (value == null)
                {
                    _contraction = null;
                    return;
                }

                _contraction = new DictataKey(new LuceneDataCacheKey(value.GetLexeme()).BirthMark, value.FormGroup);
            }
        }
    }
}
