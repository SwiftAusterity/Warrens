using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Behaviors.Existential;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Lookup Data for mobile race
    /// </summary>
    public interface IRace : ILookupData
    {
        /// <summary>
        /// The arm objects
        /// </summary>
        Tuple<IInanimateData, short> Arms { get; set; }

        /// <summary>
        /// The leg objects
        /// </summary>
        Tuple<IInanimateData, short> Legs { get; set; }

        /// <summary>
        /// the torso object
        /// </summary>
        IInanimateData Torso { get; set; }

        /// <summary>
        /// The head object
        /// </summary>
        IInanimateData Head { get; set; }

        /// <summary>
        /// The list of additional body parts used by this race. Part Object, Amount, Name
        /// </summary>
        IEnumerable<Tuple<IInanimateData, short, string>> BodyParts { get; set; }

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
        Tuple<short, short> VisionRange { get; set; }

        /// <summary>
        /// Low and High temperature range before damage starts to occur
        /// </summary>
        Tuple<short, short> TemperatureTolerance { get; set; }

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
        IGlobalPosition StartingLocation { get; set; }

        /// <summary>
        /// When a player loads without a location where do we sent them
        /// </summary>
        IGlobalPosition EmergencyLocation { get; set; }

        //TODO: Poison glands
    }
}
