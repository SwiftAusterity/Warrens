using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Backing data for IGaia, configuration settings for each zone-cluster
    /// </summary>
    [Serializable]
    public class GaiaTemplate : EntityTemplatePartial, IGaiaTemplate
    {
        /// <summary>
        /// The system type of data this attaches to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override Type EntityClass
        {
            get { return typeof(Gaia); }
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

        [JsonProperty("CelestialBodies")]
        public HashSet<TemplateCacheKey> _celestialBodies { get; set; }

        /// <summary>
        /// Celestial bodies for this world
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        [Display(Name = "Celestial Body", Description = "The Celestial bodies that orbit this world. (or the one this orbits)")]
        [UIHint("CollectionCelestialList")]
        [CelestialCollectionDataBinder]
        public HashSet<ICelestial> CelestialBodies
        {
            get
            {
                if (_celestialBodies == null)
                {
                    _celestialBodies = new HashSet<TemplateCacheKey>();
                }

                return new HashSet<ICelestial>(_celestialBodies.Select(cp => TemplateCache.Get<ICelestial>(cp)));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _celestialBodies = new HashSet<TemplateCacheKey>(value.Where(cp => cp != null).Select(cp => new TemplateCacheKey(cp)));
            }
        }

        /// <summary>
        /// Time keeping for this world
        /// </summary>
        [UIHint("Chronology")]
        public IChronology ChronologicalSystem { get; set; }

        /// <summary>
        /// The angle at which this world rotates in space. Irrelevant for fixed objects.
        /// </summary>
        [Range(0, 359, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Rotational Angle", Description = "The angle at which this world rotates in space. Irrelevant for fixed objects.")]
        [DataType(DataType.Text)]
        [Required]
        public float RotationalAngle { get; set; }

        public GaiaTemplate()
        {
            ChronologicalSystem = new Chronology();
            CelestialBodies = new HashSet<ICelestial>();
        }

        public IGaia GetLiveInstance()
        {
            return LiveCache.Get<IGaia>(Id, GetType());
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(1, 1, 1);
        }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        public IEnumerable<IZoneTemplate> GetZones()
        {
            return TemplateCache.GetAll<IZoneTemplate>().Where(zone => zone.World.Equals(this));
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("RotationalAngle", RotationalAngle.ToString());

            returnList.Add("Hours Per Day", ChronologicalSystem.HoursPerDay.ToString());
            returnList.Add("Days Per Month", ChronologicalSystem.DaysPerMonth.ToString());
            returnList.Add("Starting Year", ChronologicalSystem.StartingYear.ToString());

            foreach (string month in ChronologicalSystem.Months)
            {
                returnList.Add("Months-" + month, month);
            }

            foreach (ICelestial celestial in CelestialBodies)
            {
                returnList.Add("Celestials-" + celestial.Name, celestial.Name);
            }

            return returnList;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GaiaTemplate
            {
                Name = Name,
                Qualities = Qualities,
                RotationalAngle = RotationalAngle,
                ChronologicalSystem = ChronologicalSystem,
                CelestialBodies = CelestialBodies
            };
        }
    }
}
