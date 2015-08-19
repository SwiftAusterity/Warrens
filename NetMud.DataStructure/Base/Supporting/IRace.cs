using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Reference data for mobile race
    /// </summary>
    public interface IRace : IReferenceData
    {
        Tuple<IInanimateData, short> Arms { get; set; }

        Tuple<IInanimateData, short> Legs { get; set; }

        IInanimateData Torso { get; set; }

        IInanimateData Head { get; set; }

        IEnumerable<Tuple<IInanimateData, short, string>> BodyParts { get; set; }

        DietType DietaryNeeds { get; set; }

        IMaterial SanguinaryMaterial { get; set; }

        Tuple<short, short> VisionRange { get; set; }

        Tuple<short, short> TemperatureTolerance { get; set; }

        RespiratoryType Breathes { get; set; }

        DamageType TeethType { get; set; }

        IRoomData StartingLocation { get; set; }

        IRoomData EmergencyLocation { get; set; }

        //TODO: Poison glands
    }
}
