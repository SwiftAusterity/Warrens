using NetMud.Communication.Messaging;
using NetMud.Data.DataIntegrity;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Partial class for handling the basics of natural resources (rocks, trees, etc)
    /// </summary>
    [Serializable]
    public abstract class NaturalResourceDataPartial : LookupDataPartial, INaturalResource
    {
        /// <summary>
        /// How much spawns in one place in one spawn tick
        /// </summary>
        [IntDataIntegrity("Amount Multiplier must be between 0 and 100.", 0, 100)]
        public int AmountMultiplier { get; set; }

        /// <summary>
        /// How rare this is to spawn even in its optimal range
        /// </summary>
        [IntDataIntegrity("Rarity must be between 0 and 100.", 0, 100)]
        public int Rarity { get; set; }

        /// <summary>
        /// How much the spawned puissance varies
        /// </summary>
        [IntDataIntegrity("Puissance Variance must be between 0 and 100.", 0, 100)]
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
        [FilledContainerDataIntegrity("This resource must occur in at least one biome.")]
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

            //Tuples must be handled individually
            if (ElevationRange.Item1 < 0 || ElevationRange.Item2 < ElevationRange.Item1)
                dataProblems.Add("Elevation Range is incorrect.");

            if (TemperatureRange.Item1 < 0 || TemperatureRange.Item2 < TemperatureRange.Item1)
                dataProblems.Add("Temperature Range is incorrect.");

            if (HumidityRange.Item1 < 0 || HumidityRange.Item2 < HumidityRange.Item1)
                dataProblems.Add("Humidity Range is incorrect.");

            return dataProblems;
        }

        #region Rendering
        public virtual IEnumerable<string> RenderToLook(IEntity viewer)
        {
            var returnValues = new List<string>
            {
                GetFullShortDescription(viewer)
            };

            return returnValues;
        }

        public virtual string GetFullShortDescription(IEntity viewer)
        {
            return Name;
        }

        public virtual IEnumerable<string> RenderAsContents(IEntity viewer)
        {
            var returnValues = new List<string>
            {
                GetFullShortDescription(viewer)
            };

            return returnValues;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as Psychic output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetVisibleDescriptives()
        {
            return new Occurrence[] 
            {
                new Occurrence()
                {
                    SensoryType = MessagingType.Visible,
                    Strength = 30,
                    Event = new Lexica() {  Phrase = Name, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
                }
            };
        }

        /// <summary>
        /// Render a natural resource collection to a viewer
        /// </summary>
        /// <param name="viewer">the entity looking</param>
        /// <param name="amount">How much of it there is</param>
        /// <returns>a view string</returns>
        public virtual string RenderResourceCollection(IEntity viewer, int amount)
        {
            return string.Format("{0} {1}s", amount, GetFullShortDescription(viewer));
        }
        #endregion
    }
}
