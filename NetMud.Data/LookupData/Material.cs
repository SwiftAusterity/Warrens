using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    [Serializable]
    public class Material : LookupDataPartial, IMaterial
    {
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
        public short Viscosity { get; set; }

        /// <summary>
        /// How dense is this material
        /// </summary>
        public short Density { get; set; }

        /// <summary>
        /// How well does this material bend without breaking
        /// </summary>
        public short Mallebility { get; set; }

        /// <summary>
        /// How stretchable is this material
        /// </summary>
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

        [JsonProperty("Composition")]
        private IDictionary<long, short> _composition { get; set; }

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
                    return _composition.ToDictionary(k => BackingDataCache.Get<IMaterial>(k.Key), k => k.Value);

                return null;
            }
            set
            {
                if (value == null)
                    return;

                _composition = value.ToDictionary(k => k.Key.ID, k => k.Value);
            }
        }

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

            if (Viscosity == 0)
                dataProblems.Add("Viscosity has to be greater than 0.");

            if (Density == 0)
                dataProblems.Add("Density has to be greater than 0.");

            if (Mallebility == 0)
                dataProblems.Add("Mallebility has to be greater than 0.");

            if (Ductility == 0)
                dataProblems.Add("Ductility has to be greater than 0.");

            if (SolidPoint >= GasPoint)
                dataProblems.Add("Solidification point must be lower than gaseous point.");

            if (Resistance == null || !Resistance.Any() || Resistance.Any(r => r.Value == 0))
                dataProblems.Add("Resistances are invalid.");

            if (Composition == null || Composition.Any(r => r.Key == null || r.Value == 0))
                dataProblems.Add("Compositions are invalid.");

            return dataProblems;
        }

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            return base.RenderHelpBody();
        }
    }
}
