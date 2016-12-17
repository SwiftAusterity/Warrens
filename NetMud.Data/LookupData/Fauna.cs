using NetMud.Data.LookupData;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Existential;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Animal spawns
    /// </summary>
    [Serializable]
    public class Fauna : NaturalResourceDataPartial, IFauna
    {
        [JsonProperty("Race")]
        private long _race { get; set; }

        /// <summary>
        /// What we're spawning
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IRace Race
        {
            get
            {
                return BackingDataCache.Get<IRace>(_race);
            }
            set
            {
                _race = value.ID;
            }
        }

        /// <summary>
        /// What is the % chance of generating a female instead of a male on birth
        /// </summary>
        public int FemaleRatio { get; set; }

        /// <summary>
        /// The absolute hard cap to natural population growth
        /// </summary>
        public int PopulationHardCap { get; set; }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (Race == null)
                dataProblems.Add("Race must be set.");

            if (PopulationHardCap <= 0)
                dataProblems.Add("Population Hard Cap must be greater than 0.");

            if (FemaleRatio <= 0)
                dataProblems.Add("Female to male ratio must be greater than 0.");

            return dataProblems;
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
