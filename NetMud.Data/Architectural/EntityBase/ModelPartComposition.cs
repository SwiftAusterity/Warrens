using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// What model parts are made of
    /// </summary>
    [Serializable]
    public class ModelPartComposition : IModelPartComposition
    {
        /// <summary>
        /// The name of the part section on the model
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Part", Description = "The name of the part section.")]
        public string PartName { get; set; }

        [JsonProperty("Material")]
        private TemplateCacheKey _material { get; set; }

        /// <summary>
        /// The material it's made of
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [UIHint("MaterialList")]
        [Display(Name = "Material", Description = "What the part section is made of.")]
        [MaterialDataBinder]
        public IMaterial Material
        {
            get
            {
                if (_material != null)
                    return TemplateCache.Get<IMaterial>(_material);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _material = new TemplateCacheKey(value);
            }
        }
    }
}
