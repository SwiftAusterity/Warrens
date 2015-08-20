using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Pathways
    /// </summary>
    public interface IPathwayData : IEntityBackingData
    {
        /// <summary>
        /// How wide this pathway portal is
        /// </summary>
        long PassingWidth { get; set; }

        /// <summary>
        /// How high the pathway portal is
        /// </summary>
        long PassingHeight { get; set; }

        /// <summary>
        /// 0->360 degrees with 0 being absolute north (meaning 90 is west, 180 south, etc) -1 means no cardinality
        /// </summary>
        int DegreesFromNorth { get; set; }

        /// <summary>
        /// The container this points into
        /// </summary>
        string ToLocationID { get; set; }

        /// <summary>
        /// The system type of the container this points into
        /// </summary>
        string ToLocationType { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        string FromLocationID { get; set; }

        /// <summary>
        /// The system type of the container this starts in
        /// </summary>
        string FromLocationType { get; set; }

        /// <summary>
        /// Output message format the Actor recieves upon moving
        /// </summary>
        string MessageToActor { get; set; }

        /// <summary>
        /// Output message format the originating location's entities recieve upon moving
        /// </summary>
        string MessageToOrigin { get; set; }

        /// <summary>
        /// Output message format the destination location's entities recieve upon moving
        /// </summary>
        string MessageToDestination { get; set; }

        /// <summary>
        /// Audible (heard) message sent to surrounding locations of both origin and destination
        /// </summary>
        string AudibleToSurroundings { get; set; }

        /// <summary>
        /// Strength of audible message to surroundings
        /// </summary>
        int AudibleStrength { get; set; }

        /// <summary>
        /// Visible message sent to surrounding locations of both origin and destination
        /// </summary>
        string VisibleToSurroundings { get; set; }

        /// <summary>
        /// Strength of visible message to surroundings
        /// </summary>
        int VisibleStrength { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
