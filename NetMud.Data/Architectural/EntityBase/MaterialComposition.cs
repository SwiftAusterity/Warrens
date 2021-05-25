using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Architectural.EntityBase
{
    [Serializable]
    public class MaterialComposition : IMaterialComposition
    {
        /// <summary>
        /// how much of the alloy is this material (1 to 100)
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Percentage", Description = "How much of the alloy is this material (1 to 100).")]
        public short PercentageOfComposition { get; set; }

        [JsonProperty("Material")]
        public TemplateCacheKey _material { get; set; }

        /// <summary>
        /// The material it's made of
        /// </summary>

        [JsonIgnore]
        [UIHint("MaterialList")]
        [Display(Name = "Material", Description = "The material it's made of.")]
        [MaterialDataBinder]
        public IMaterial Material
        {
            get
            {
                if (_material != null)
                {
                    return TemplateCache.Get<IMaterial>(_material);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _material = new TemplateCacheKey(value);
            }
        }
    }
}
