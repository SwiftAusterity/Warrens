using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Rules that identify when words convert to other words based on placement
    /// </summary>
    public class DictataTransformationRule : IDictataTransformationRule
    {
        /// <summary>
        /// The word to be transformed
        /// </summary>
        [JsonProperty("Origin")]
        private ConfigDataCacheKey _origin { get; set; }

        /// <summary>
        /// When the from word is specifically this
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Origin Word", Description = "When the origin word is specifically this.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata Origin
        {
            get
            {
                if (_origin == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_origin);
            }
            set
            {
                if (value == null)
                {
                    _origin = null;
                    return;
                }

                _origin = new ConfigDataCacheKey(value);
            }
        }

        [JsonProperty("SpecificFollowing")]
        private ConfigDataCacheKey _specificFollowing { get; set; }

        /// <summary>
        /// When the from word is specifically this
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Specific Word", Description = "When the following word is specifically this.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata SpecificFollowing
        {
            get
            {
                if (_specificFollowing == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_specificFollowing);
            }
            set
            {
                if (value == null)
                {
                    _specificFollowing = null;
                    return;
                }

                _specificFollowing = new ConfigDataCacheKey(value);
            }
        }

        /// <summary>
        /// Only when the following word ends with this string
        /// </summary>
        [Display(Name = "Ends With", Description = "Only when the following word ends with this string.")]
        [DataType(DataType.Text)]
        public string EndsWith { get; set; }

        /// <summary>
        /// Only when the following word begins with this string
        /// </summary>
        [Display(Name = "Begins With", Description = "Only when the following word begins with this string.")]
        [DataType(DataType.Text)]
        public string BeginsWith { get; set; }

        /// <summary>
        /// The word this turns into
        /// </summary>
        [JsonProperty("TransformedWord")]
        private ConfigDataCacheKey _transformedWord { get; set; }

        /// <summary>
        /// The word this turns into
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Transformed Word", Description = "The word this turns into.")]
        [UIHint("DictataList")]
        [DictataDataBinder]
        public IDictata TransformedWord
        {
            get
            {
                if (_transformedWord == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<IDictata>(_transformedWord);
            }
            set
            {
                if (value == null)
                {
                    _transformedWord = null;
                    return;
                }

                _transformedWord = new ConfigDataCacheKey(value);
            }
        }

        public DictataTransformationRule()
        {
            BeginsWith = string.Empty;
            EndsWith = string.Empty;
        }
    }
}
