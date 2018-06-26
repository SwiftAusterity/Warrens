using NetMud.Communication.Messaging;
using NetMud.Data.DataIntegrity;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Partial class for handling the basics of natural resources (rocks, trees, etc)
    /// </summary>
    [Serializable]
    public abstract class NaturalResourceDataPartial : LookupDataPartial, INaturalResource
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

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

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            var returnList = base.SignificantDetails();

            returnList.Add("Amount Multiplier", AmountMultiplier.ToString());
            returnList.Add("Rarity", Rarity.ToString());
            returnList.Add("Puissance Variance", PuissanceVariance.ToString());
            returnList.Add("Elevation", string.Format("{0} - {1}", ElevationRange.Item1, ElevationRange.Item2));
            returnList.Add("Temperature", string.Format("{0} - {1}", TemperatureRange.Item1, TemperatureRange.Item2));
            returnList.Add("Humidity", string.Format("{0} - {1}", HumidityRange.Item1, HumidityRange.Item2));

            foreach (var occur in OccursIn)
                returnList.Add("Occurs In", occur.ToString());

            foreach (var affect in Affects)
                returnList.Add("Affect", string.Format("{0} ({1})", affect.Target, affect.Duration));

            return returnList;
        }

        #region Rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> RenderToLook(IEntity viewer)
        {
            //if (!IsVisibleTo(viewer))
            //    return Enumerable.Empty<string>();

            return GetLongDescription(viewer);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> GetLongDescription(IEntity viewer)
        {
            //if (!IsVisibleTo(viewer))
            //    return Enumerable.Empty<string>();

            var sb = new List<string>();
            var descriptives = GetVisibleDescriptives(viewer);
            sb.AddRange(descriptives.Select(desc => desc.Event.ToString()));

            return sb;
        }

        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> RenderAsContents(IEntity viewer)
        {
            //if (!IsVisibleTo(viewer))
            //    return Enumerable.Empty<string>();

            return GetShortDescription(viewer);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<string> GetShortDescription(IEntity viewer)
        {
            //if (!IsVisibleTo(viewer))
            //    return Enumerable.Empty<string>();

            return new List<string> { Name }; ;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribedName(IEntity viewer)
        {
            //if (!IsVisibleTo(viewer))
            //    return string.Empty;

            return Name;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<IOccurrence> GetVisibleDescriptives(IEntity viewer)
        {
            //TODO: Check for visibility, and also list descriptives
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
            return string.Format("{0} {1}s", amount, GetDescribedName(viewer));
        }
        #endregion
    }
}
