using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    [Serializable]
    public class NaturalResourceDataPartial : LookupDataPartial, INaturalResource
    {
        /// <summary>
        /// How much spawns in one place in one spawn tick
        /// </summary>
        public int AmountMultiplier { get; set; }

        /// <summary>
        /// How rare this is to spawn even in its optimal range
        /// </summary>
        public int Rarity { get; set; }

        /// <summary>
        /// How much the spawned puissance varies
        /// </summary>
        public int PuissanceVariance { get; set; }

        /// <summary>
        /// Spawns in elevations within this range
        /// </summary>
        public Tuple<int, int> ElevationRange { get; set; }

        /// <summary>
        /// Spawns in temperatures within this range
        /// </summary>
        public Tuple<int, int> TemperatureRange { get; set; }

        /// <summary>
        /// Spawns in humidities within this range
        /// </summary>
        public Tuple<int, int> HumidityRange { get; set; }

        [JsonProperty("OccursIn")]
        private IEnumerable<long> _occursIn { get; set; }

        /// <summary>
        /// What medium materials this can spawn in
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IMaterial> OccursIn
        {
            get
            {
                if (_occursIn == null)
                    _occursIn = new HashSet<long>();

                return BackingDataCache.GetMany<IMaterial>(_occursIn);
            }
            set
            {
                _occursIn =value.Select(m => m.ID);
            }
        }

        /// <summary>
        /// The affects.. affecting the entity
        /// </summary>
        public HashSet<IAffect> Affects { get; private set; }

        /// <summary>
        /// Checks if there is an affect without having to crawl the hashset everytime or returning a big class object
        /// </summary>
        /// <param name="affectTarget">the target of the affect</param>
        /// <returns>if it exists or not</returns>
        public bool HasAffect(string affectTarget)
        {
            return Affects.Any(aff => aff.Target.Equals(affectTarget, StringComparison.InvariantCultureIgnoreCase)
                            && (aff.Duration > 0 || aff.Duration == -1));
        }

        /// <summary>
        /// Can spawn in system zones like non-player owned cities
        /// </summary>
        public bool CanSpawnInSystemAreas { get; set; }

        /// <summary>
        /// Can this resource potentially spawn in this room
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this can spawn there</returns>
        public bool CanSpawnIn(IRoom room)
        {
            //TODO : This
            return true;
        }

        /// <summary>
        /// Should this resource spawn in this room. Combines the "can" logic with checks against total local population
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this should spawn there</returns>
        public bool ShouldSpawnIn(IRoom room)
        {
            //TODO : This
            return true;
        }
    }
}
