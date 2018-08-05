using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Backing data for IGaia, configuration settings for each zone-cluster
    /// </summary>
    public interface IGaiaData : IEntityBackingData, ISingleton<IGaia>
    {
        /// <summary>
        /// Celestial bodies for this world
        /// </summary>
        IEnumerable<ICelestial> CelestialBodies { get; set; }

        /// <summary>
        /// Time keeping for this world
        /// </summary>
        IChronology ChronologicalSystem { get; set; }

        /// <summary>
        /// The angle at which this world rotates in space. Irrelevant for fixed objects.
        /// </summary>
        float RotationalAngle { get; set; }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        IEnumerable<IZoneData> GetZones();
    }
}
