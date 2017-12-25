using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// What medium biomes this can spawn in
        /// </summary>
        public HashSet<Biome> OccursIn { get; set; }

        /// <summary>
        /// The affects.. affecting the entity
        /// </summary>
        public HashSet<IAffect> Affects { get; set; }

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
        public virtual bool CanSpawnIn(IGlobalPosition position)
        {
            return true;
        }

        /// <summary>
        /// Should this resource spawn in this room. Combines the "can" logic with checks against total local population
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this should spawn there</returns>
        public virtual bool ShouldSpawnIn(IGlobalPosition room)
        {
            return true;
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            var dataProblems = base.FitnessReport();

            if (AmountMultiplier < 0 || AmountMultiplier > 100)
                dataProblems.Add("Amount Multiplier must be between 0 and 100.");

            if (Rarity < 0 || Rarity > 100)
                dataProblems.Add("Rarity must be between 0 and 100.");

            if (PuissanceVariance < 0 || PuissanceVariance > 100)
                dataProblems.Add("Puissance Variance must be between 0 and 100.");

            if (ElevationRange.Item1 < 0 || ElevationRange.Item2 < ElevationRange.Item1)
                dataProblems.Add("Elevation Range is incorrect.");

            if (TemperatureRange.Item1 < 0 || TemperatureRange.Item2 < TemperatureRange.Item1)
                dataProblems.Add("Temperature Range is incorrect.");

            if (HumidityRange.Item1 < 0 || HumidityRange.Item2 < HumidityRange.Item1)
                dataProblems.Add("Humidity Range is incorrect.");

            if (OccursIn == null || OccursIn.Count() == 0)
                dataProblems.Add("This resource must occur in at least one biome.");

            return dataProblems;
        }
    }
}
