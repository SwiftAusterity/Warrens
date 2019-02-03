using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Zone
{

    public class ZoneTemplate : LocationTemplateEntityPartial, IZoneTemplate
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override Type EntityClass
        {
            get { return typeof(Zone); }
        }

        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// keywords this entity is referrable by in the world by the parser
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public override string[] Keywords
        {
            get
            {
                if (_keywords == null || _keywords.Length == 0)
                {
                    _keywords = new string[] { Name.ToLower() };
                }

                return _keywords;
            }
            set { _keywords = value; }
        }

        /// <summary>
        /// What hemisphere this zone is in
        /// </summary>
        [Display(Name = "Hemisphere", Description = "The hemisphere of the world this zone is in.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public HemispherePlacement Hemisphere { get; set; }

        /// <summary>
        /// Temperature variance for generating locales
        /// </summary>
        [Display(Name = "Temperature Coefficient", Description = "Determines how chaotic the weather systems are over this zone.")]
        [DataType(DataType.Text)]
        public int TemperatureCoefficient { get; set; }

        /// <summary>
        /// Barometric variance for generating locales
        /// </summary>
        [Display(Name = "Barometric Coefficient", Description = "Determines how chaotic the weather systems are over this zone.")]
        [DataType(DataType.Text)]
        public int PressureCoefficient { get; set; }

        [Display(Name = "Base Elevation", Description = "What the central elevation is.")]
        [DataType(DataType.Text)]
        public int BaseElevation { get; set; }

        /// <summary>
        /// What the natural biome is for generating locales
        /// </summary>
        [Display(Name = "Biome", Description = "The biome of the zone.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public Biome BaseBiome { get; set; }

        /// <summary>
        /// Is this zone always discovered by players (ie no need to be discovered)
        /// </summary>
        [Display(Name = "Default Discovered", Description = "Is this zone always discovered by players (ie no need to be discovered).")]
        [UIHint("Boolean")]
        public bool AlwaysDiscovered { get; set; }

        [JsonProperty("World")]
        private TemplateCacheKey _world { get; set; }

        /// <summary>
        /// What world does this belong to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "World", Description = "The World/Dimension this belongs to.")]
        [UIHint("GaiaTemplateList")]
        [GaiaTemplateDataBinder]
        [Required]
        public IGaiaTemplate World
        {
            get
            {
                if (_world == null)
                {
                    return null;
                }

                return TemplateCache.Get<IGaiaTemplate>(_world);
            }
            set
            {
                if (value != null)
                {
                    _world = new TemplateCacheKey(value);
                }
            }
        }

        /// <summary>
        /// Paths out of this zone
        /// </summary>
        public HashSet<IPathway> Pathways { get; set; }

        [JsonProperty("Templates")]
        private HashSet<TemplateCacheKey> _templates { get; set; }

        /// <summary>
        /// Adventure templates valid for this zone
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public HashSet<IAdventureTemplate> Templates
        {
            get
            {
                if (_templates != null)
                {
                    return new HashSet<IAdventureTemplate>(TemplateCache.GetMany<IAdventureTemplate>(_templates));
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _templates = new HashSet<TemplateCacheKey>(value.Select(k => new TemplateCacheKey(k)));
            }
        }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Natural Resource Spawn", Description = "Spawn rates for natural resources.")]
        [UIHint("NaturalResourceSpawnList")]
        public HashSet<INaturalResourceSpawn> NaturalResourceSpawn { get; set; }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public ZoneTemplate()
        {
            Templates = new HashSet<IAdventureTemplate>();
            NaturalResourceSpawn = new HashSet<INaturalResourceSpawn>();
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Temperature", TemperatureCoefficient.ToString());
            returnList.Add("Pressure", PressureCoefficient.ToString());
            returnList.Add("Biome", BaseBiome.ToString());
            returnList.Add("Hemisphere", Hemisphere.ToString());

            return returnList;
        }

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public override bool PersistToCache()
        {
            try
            {
                TemplateCache.Add(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            HashSet<IPathway> pathways = new HashSet<IPathway>();
            foreach (IPathway pathway in Pathways)
            {
                pathways.Add((IPathway)pathway.Clone());
            }

            return new ZoneTemplate
            {
                Name = Name,
                Qualities = Qualities,
                BaseBiome = BaseBiome,
                Hemisphere = Hemisphere,
                PressureCoefficient = PressureCoefficient,
                TemperatureCoefficient = TemperatureCoefficient,
                World = World,
                Pathways = pathways
            };
        }

        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(1, 1, 1);
        }

        public IZone GetLiveInstance()
        {
            return LiveCache.Get<IZone>(Id);
        }
    }
}
