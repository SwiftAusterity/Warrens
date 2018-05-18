using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Pathways
    /// </summary>
    public interface IPathwayData : IEntityBackingData, ISingleton
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
        /// The container this points into
        /// </summary>
        IRoomData ToLocation { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        IRoomData FromLocation { get; set; }

        /// <summary>
        /// The visual output of using this path
        /// </summary>
        ILexica VisualOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        ILexica AuditoryOutput { get; set; }

        /// <summary>
        /// The auditory output of using this path
        /// </summary>
        ILexica OlefactoryOutput { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; set; }
    }
}
