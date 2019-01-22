using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Lookup Data for mobile race
    /// </summary>
    public interface IRace : ILookupData
    {
        /// <summary>
        /// The arm objects
        /// </summary>
        IInanimateComponent Arms { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        IInanimateComponent Legs { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>
        IInanimateTemplate Torso { get; set; }

        /// <summary>
        /// The head object
        /// </summary>
        IInanimateTemplate Head { get; set; }

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        HashSet<BodyPart> BodyParts { get; set; }

        /// <summary>
        /// Dietary type of this race
        /// </summary>       
        DietType DietaryNeeds { get; set; }

        /// <summary>
        /// Material that is the blood
        /// </summary>
        IMaterial SanguinaryMaterial { get; set; }

        /// <summary>
        /// Low and High luminosity vision range
        /// </summary>
        ValueRange<short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        ValueRange<short> TemperatureTolerance { get; set; }

        /// <summary>
        /// What mode of breathing
        /// </summary>
        RespiratoryType Breathes { get; set; }

        /// <summary>
        /// The type of damage biting inflicts
        /// </summary>
        DamageType TeethType { get; set; }

        /// <summary>
        /// What is the starting room of new players
        /// </summary>
        IZoneTemplate StartingLocation { get; set; }

        /// <summary>
        /// When a player loads without a location where do we sent them
        /// </summary>
        IZoneTemplate EmergencyLocation { get; set; }

        /// <summary>
        /// The name used to describe a large gathering of this race
        /// </summary>       
        string CollectiveNoun { get; set; }

        /// <summary>
        /// Method to get the full list of anatomical features of this race
        /// </summary>
        IEnumerable<BodyPart> FullAnatomy();

        /// <summary>
        /// Render this race's body as an ascii.. thing
        /// </summary>
        /// <returns>List of strings as rows for rendering</returns>
        IEnumerable<string> RenderAnatomy(bool forWeb);

        //TODO: Poison glands
    }
}
