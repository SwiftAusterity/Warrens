using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Backing data for Pathways
    /// </summary>
    public interface IPathwayFramework : IDescribable, ISingleton<IPathway>
    {
        /// <summary>
        /// DegreesFromNorth translated
        /// </summary>
        MovementDirectionType DirectionType { get; }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        int DegreesFromNorth { get; set; }

        /// <summary>
        /// -100 to 100 (negative being a decline) % grade of up and down
        /// </summary>
        int InclineGrade { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; set; }
    }
}
