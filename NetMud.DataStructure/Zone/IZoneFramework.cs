using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZoneFramework
    {
        /// <summary>
        /// The entrance coordinates if someone ends up in this zone without entrance coordinates
        /// </summary>
        Coordinate BaseCoordinates { get; set; }

        /// <summary>
        /// Paths out of this zone
        /// </summary>
        HashSet<IPathway> Pathways { get; set; }
    }
}
