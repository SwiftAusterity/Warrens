using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Zones
{
    /// <summary>
    /// Backing data for pathways
    /// </summary>
    [Serializable]
    public class PathwayData : IPathway
    {
        /// <summary>
        /// Which tile the origin to this path is
        /// </summary>
        public Coordinate OriginCoordinates { get; set; }

        /// <summary>
        /// The color of the border the tile will get
        /// </summary>
        public string BorderHexColor { get; set; }

        /// <summary>
        /// The zones plus coordinates this can lead to
        /// </summary>
        public HashSet<IPathwayDestination> Destinations { get; set; }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public object Clone()
        {
            return new PathwayData
            {
                OriginCoordinates = OriginCoordinates,
                BorderHexColor = BorderHexColor,
                Destinations = Destinations
            };
        }
    }
}
