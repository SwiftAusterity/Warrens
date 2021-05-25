using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Zone
{
    [Serializable]
    public class FloraResourceSpawn : INaturalResourceSpawn<IFlora>
    {
        [JsonProperty("Resource")]
        private TemplateCacheKey _resource { get; set; }

        /// <summary>
        /// The resource at hand
        /// </summary>

        [JsonIgnore]
        [Display(Name = "Resource", Description = "The resource that will spawn.")]
        [UIHint("FloraResourceList")]
        [FloraResourceBinder]
        public IFlora Resource
        {
            get
            {
                if (_resource != null)
                {
                    return TemplateCache.Get<IFlora>(_resource);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _resource = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// The factor in how much and how frequently these respawn on their own
        /// </summary>
        [Display(Name = "Rate", Description = "The factor in how much and how frequently these respawn on their own.")]
        [DataType(DataType.Text)]
        public int RateFactor { get; set; }
    }
}
