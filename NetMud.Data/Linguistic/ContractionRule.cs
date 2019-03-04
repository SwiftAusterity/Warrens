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
