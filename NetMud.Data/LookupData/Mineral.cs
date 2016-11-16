using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
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
    /// Rocks, minable metals and dirt
    /// </summary>
    [Serializable]
    public class Mineral : NaturalResourceDataPartial, IMineral
    {
        [JsonProperty("Rock")]
        private long _rock { get; set; }

        /// <summary>
        /// What is the solid, crystallized form of this
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMaterial Rock
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_rock);
            }
            set
            {
                _rock = value.ID;
            }
        }

        [JsonProperty("Dirt")]
        private long _dirt { get; set; }

        /// <summary>
        /// What is the scattered, ground form of this
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMaterial Dirt
        {
            get
            {
                return BackingDataCache.Get<IMaterial>(_dirt);
            }
            set
            {
                _dirt = value.ID;
            }
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (Dirt == null)
                dataProblems.Add("Dirt must have a value.");

            if (Rock == null)
                dataProblems.Add("Rock must have a value.");

            return dataProblems;
        }
    }
}
