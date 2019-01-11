using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NaturalResource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Rocks, minable metals and dirt
    /// </summary>
    [Serializable]
    public class Mineral : NaturalResourceDataPartial, IMineral
    {
        /// <summary>
        /// How soluble the dirt is
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Solubility", Description = "The factor of how well this dissolves in water.")]
        [DataType(DataType.Text)]
        public int Solubility { get; set; }

        /// <summary>
        /// How fertile the dirt generally is
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Dirt Fertility", Description = "How likely are fauna to grow in this if it is used as dirt.")]
        [DataType(DataType.Text)]
        public int Fertility { get; set; }

        [JsonProperty("Rock")]
        private TemplateCacheKey _rock { get; set; }

        /// <summary>
        /// What is the solid, crystallized form of this
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Rock must have a value.")]
        [Display(Name = "Rock", Description = "What object is used to refer to this in rock form.")]
        public IMaterial Rock
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_rock);
            }
            set
            {
                _rock = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Dirt")]
        private TemplateCacheKey _dirt { get; set; }

        /// <summary>
        /// What is the scattered, ground form of this
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [NonNullableDataIntegrity("Dirt must have a value.")]
        [Display(Name = "Dirt", Description = "What object is used to refer to this in dirt form.")]
        public IMaterial Dirt
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_dirt);
            }
            set
            {
                _dirt = new TemplateCacheKey(value);
            }
        }

        [JsonProperty("Ores")]
        private IEnumerable<TemplateCacheKey> _ores { get; set; }

        /// <summary>
        /// What medium minerals this can spawn in
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Ores", Description = "What ores this contains when mined as rock.")]
        public IEnumerable<IMineral> Ores
        {
            get
            {
                if (_ores == null)
                    _ores = new HashSet<TemplateCacheKey>();

                return TemplateCache.GetMany<IMineral>(_ores);
            }
            set
            {
                _ores = value.Select(m => new TemplateCacheKey(m));
            }
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Solubility", Solubility.ToString());
            returnList.Add("Fertility", Fertility.ToString());
            returnList.Add("Rock", Rock.Name);
            returnList.Add("Dirt", Dirt.ToString());

            foreach(var ore in Ores)
                returnList.Add("Ore", ore.Name);

            return returnList;
        }

        public override bool CanSpawnIn(IGlobalPosition location)
        {
            var returnValue = true;

            return base.CanSpawnIn(location) && returnValue;
        }

        public override bool ShouldSpawnIn(IGlobalPosition location)
        {
            var returnValue = true;

            return base.ShouldSpawnIn(location) && returnValue;
        }
    }
}
