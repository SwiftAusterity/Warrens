using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Backing data for Pathways
    /// </summary>
    public interface IPathway : ICloneable
    {
        /// <summary>
        /// Tile coordinates the pathway can be accessed from
        /// </summary>
        Coordinate OriginCoordinates { get; set; }

        /// <summary>
        /// The color of the border the tile will get
        /// </summary>
        string BorderHexColor { get; set; }

        /// <summary>
        /// The zones plus coordinates this can lead to
        /// </summary>
        HashSet<IPathwayDestination> Destinations { get; set; }
    }
}
