using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    [Serializable]
    public class Material : LookupDataPartial, IMaterial
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// Is this material energy conduction
        /// </summary>
        [Display(Name = "Conductive", Description = "Does this conduct electricity?")]
        [UIHint("Boolean")]
        public bool Conductive { get; set; }

        /// <summary>
        /// Is this material magnetic
        /// </summary>
        [Display(Name = "Magnetic", Description = "Is this magnetic?")]
        [UIHint("Boolean")]
        public bool Magnetic { get; set; }

        /// <summary>
        /// Is this material flammable
        /// </summary>
        [Display(Name = "Flammable", Description = "Is this flammable?")]
        [UIHint("Boolean")]
        public bool Flammable { get; set; }

        /// <summary>
        /// How viscous is this material (higher = more viscous)
        /// </summary>
        [ShortDataIntegrity("Viscosity has to be greater than 0.", 0)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Viscosity", Description = "How easily this material flows across other materials. High viscosity = flows less easily.")]
        [DataType(DataType.Text)]
        public short Viscosity { get; set; }

        /// <summary>
        /// How dense is this material
        /// </summary>
        [ShortDataIntegrity("Density has to be greater than 0.", 0)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Density", Description = "The density of this material. High density = higher weight of objects. Also factors into damage resistance.")]
        [DataType(DataType.Text)]
        public short Density { get; set; }

        /// <summary>
        /// How well does this material bend without breaking
        /// </summary>
        [ShortDataIntegrity("Mallebility has to be greater than 0.", 0)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Mallebility", Description = "How easily this material is deformed by impact damage without breaking or fracturing.")]
        [DataType(DataType.Text)]
        public short Mallebility { get; set; }

        /// <summary>
        /// How stretchable is this material
        /// </summary>
        [ShortDataIntegrity("Ductility has to be greater than 0.", 0)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Ductility", Description = "How easily this material is deformed by tensile stress (stretching, bending) without breaking.")]
        [DataType(DataType.Text)]
        public short Ductility { get; set; }

        /// <summary>
        /// How porous is this material
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Porosity", Description = "How porous (full of holes) this material is when formed. Highly porous materials may become pass thru for small objects and creatures.")]
        [DataType(DataType.Text)]
        public short Porosity { get; set; }

        /// <summary>
        /// What is the freezing point of this material
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Fusion Point", Description = "The temperature at which this material becomes a solid.")]
        [DataType(DataType.Text)]
        public short SolidPoint { get; set; }

        /// <summary>
        /// What is the temperature gasous point of this material
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Vaporization Point", Description = "The temperature at which this material becomes a gas.")]
        [DataType(DataType.Text)]
        public short GasPoint { get; set; }

        /// <summary>
        /// How well does this material hold temperature changes
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature Retention", Description = "How well does this material retain its temperature.")]
        [DataType(DataType.Text)]
        public short TemperatureRetention { get; set; }

        /// <summary>
        /// Any elemental resistances the material has
        /// </summary>
        [Display(Name = "Damage Resistance", Description = "The type of damage this has special resistances to.")]
        public IDictionary<DamageType, short> Resistance { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<IOccurrence> Descriptives { get; set; }

        [JsonProperty("Composition")]
        private IDictionary<TemplateCacheKey, short> _composition { get; set; }

        /// <summary>
        /// Collection of model section name to material composition mappings
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Material Composition", Description = "Is this material an alloy of other materials?")]
        public IDictionary<IMaterial, short> Composition
        {
            get
            {
                if (_composition != null)
                    return _composition.ToDictionary(k => TemplateCache.Get<IMaterial>(k.Key), k => k.Value);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _composition = value.ToDictionary(k => new TemplateCacheKey(k.Key), k => k.Value);
            }
        }

        [Display(Name = "Accumulation Cap", Description = "How many of this can go in one 'stack'.")]
        [Range(0, 999, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        public int AccumulationCap { get; set; }

        /// <summary>
        /// Make a new empty instance of this
        /// </summary>
        public Material()
        {
            Resistance = new Dictionary<DamageType, short>();
            Composition = new Dictionary<IMaterial, short>();
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (SolidPoint >= GasPoint)
                dataProblems.Add("Solidification point must be lower than gaseous point.");

            //Specific interior value checking
            if (Resistance == null || !Resistance.Any() || Resistance.Any(r => r.Value == 0))
                dataProblems.Add("Resistances are invalid.");

            if (Composition == null || Composition.Any(r => r.Key == null || r.Value == 0))
                dataProblems.Add("Compositions are invalid.");

            return dataProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Conductive", Conductive.ToString());
            returnList.Add("Magnetic", Magnetic.ToString());
            returnList.Add("Flammable", Flammable.ToString());
            returnList.Add("Viscosity", Viscosity.ToString());
            returnList.Add("Density", Density.ToString());
            returnList.Add("Mallebility", Mallebility.ToString());
            returnList.Add("Ductility", Ductility.ToString());
            returnList.Add("Porosity", Porosity.ToString());
            returnList.Add("Solid Point", SolidPoint.ToString());
            returnList.Add("Gas Point", GasPoint.ToString());
            returnList.Add("Temperature Retention", TemperatureRetention.ToString());

            foreach (var desc in Descriptives)
                returnList.Add("Descriptives", string.Format("{0} ({1}): {2}", desc.SensoryType, desc.Strength, desc.Event.ToString()));

            return returnList;
        }
    }
}
