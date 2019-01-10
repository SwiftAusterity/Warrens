using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public bool Conductive { get; set; }

        /// <summary>
        /// Is this material magnetic
        /// </summary>
        public bool Magnetic { get; set; }

        /// <summary>
        /// Is this material flammable
        /// </summary>
        public bool Flammable { get; set; }

        /// <summary>
        /// How viscous is this material (higher = more viscous)
        /// </summary>
        [ShortDataIntegrity("Viscosity has to be greater than 0.", 0)]
        public short Viscosity { get; set; }

        /// <summary>
        /// How dense is this material
        /// </summary>
        [ShortDataIntegrity("Density has to be greater than 0.", 0)]
        public short Density { get; set; }

        /// <summary>
        /// How well does this material bend without breaking
        /// </summary>
        [ShortDataIntegrity("Mallebility has to be greater than 0.", 0)]
        public short Mallebility { get; set; }

        /// <summary>
        /// How stretchable is this material
        /// </summary>
        [ShortDataIntegrity("Ductility has to be greater than 0.", 0)]
        public short Ductility { get; set; }

        /// <summary>
        /// How porous is this material
        /// </summary>
        public short Porosity { get; set; }

        /// <summary>
        /// What is the freezing point of this material
        /// </summary>
        public short SolidPoint { get; set; }

        /// <summary>
        /// What is the temperature gasous point of this material
        /// </summary>
        public short GasPoint { get; set; }

        /// <summary>
        /// How well does this material hold temperature changes
        /// </summary>
        public short TemperatureRetention { get; set; }

        /// <summary>
        /// Any elemental resistances the material has
        /// </summary>
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
